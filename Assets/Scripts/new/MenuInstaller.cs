using System.ComponentModel;
using Zenject;

public class MenuInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // ����������� ��������
        Container.Bind<IRecordService>().To<RecordService>().AsSingle();
        Container.Bind<IGameResultRepository>().To<GameResultRepository>().AsSingle();
        Container.Bind<ISceneLoader>().To<SceneLoader>().FromComponentInHierarchy().AsSingle();
        // ����������� ������������
        Container.BindInterfacesAndSelfTo<MenuController>().FromComponentInHierarchy().AsSingle();
       
    }
}
