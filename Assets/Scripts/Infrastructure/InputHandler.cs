using UnityEngine;
using Zenject;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class InputHandler : Zenject.IInitializable, ITickable
{
    private readonly IGameManager _gameManager;
    private Camera _mainCamera;
    private bool _isInputBlocked;

    private const float DOUBLE_CLICK_THRESHOLD = 0.3f;
    private float _lastClickTime;

    [Inject] private readonly CoroutineRunner _coroutineRunner;

    [Inject] private readonly ObjectPool<Ring> _ringPool;
    [Inject(Id = "TransparentMaterial")] private readonly Material _transparentMaterial;

    public InputHandler(IGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Initialize()
    {
        _mainCamera = Camera.main;
        _isInputBlocked = false;
    }

    public void Tick()
    {
        if (_gameManager.IsGameOver || _isInputBlocked) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            HandleInput(touch.position);
        }
    }

    private void HandleInput(Vector2 inputPosition)
    {
        if (Time.time - _lastClickTime < DOUBLE_CLICK_THRESHOLD)
        {
            _isInputBlocked = true;
            _coroutineRunner.Run(ResetInputBlock());
            return;
        }
        _lastClickTime = Time.time;

        Ray ray = _mainCamera.ScreenPointToRay(inputPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            _gameManager.SelectRing(null);
            return;
        }

        ProcessHitObject(hit.collider);
    }

    private void ProcessHitObject(Collider collider)
    {
        Ring ring = collider.GetComponent<Ring>();
        RingPlaceholder placeholder = collider.GetComponent<RingPlaceholder>();

        if (ring != null)
        {
            HandleRingClick(ring);
        }
        else if (placeholder != null)
        {
            HandlePlaceholderClick(placeholder);
        }
        else
        {
            _gameManager.SelectRing(null);
        }
    }

    private void HandleRingClick(Ring ring)
    {
        if (ring.IsTransparent)
        {
            HandleTransparentRingClick(ring);
        }
        else
        {
            HandleNormalRingClick(ring);
        }
    }

    private void HandleNormalRingClick(Ring ring)
    {
        if (ring.CurrentTower == null ||
            ring.CurrentTower.Rings.Count == 0 ||
            ring.CurrentTower.Rings[^1] != ring) return;

        _gameManager.SelectRing(ring);
        PlaySelectionEffect(ring.transform);
    }

    private void HandleTransparentRingClick(Ring ring)
    {
        RingPlaceholder placeholder = ring.GetComponentInParent<RingPlaceholder>();
        if (placeholder != null)
        {
            _gameManager.MoveRing(placeholder);
            PlayMoveEffect(ring.transform);
        }
    }

    private void HandlePlaceholderClick(RingPlaceholder placeholder)
    {
        if (_gameManager.Moves == 0) return;

        Ring transparentRing = _ringPool.Get();
        transparentRing.Initialize(_transparentMaterial.color);
        transparentRing.IsTransparent = true;
        transparentRing.transform.position = placeholder.transform.position;

        _gameManager.MoveRing(placeholder);
    }

    private void PlaySelectionEffect(Transform target)
    {
        _isInputBlocked = true;
        target.DOPunchScale(Vector3.one * 0.2f, 0.2f)
              .OnComplete(() => _isInputBlocked = false);
    }

    private void PlayMoveEffect(Transform target)
    {
        target.DOJump(target.position, 0.5f, 1, 0.3f)
              .SetEase(Ease.OutQuad);
    }

    private IEnumerator ResetInputBlock()
    {
        yield return new WaitForSeconds(0.5f);
        _isInputBlocked = false;
    }
}

public class CoroutineRunner : MonoBehaviour
{
    public void Run(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
