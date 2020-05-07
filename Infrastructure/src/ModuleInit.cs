using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CommonServiceLocator;
using Domain.Services;
using Infrastructure.Db;
using Infrastructure.Messaging;
using Infrastructure.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using WorkTimeAlghorithm;

namespace Infrastructure.src
{
    internal static class ModuleInit
    {
        [Conditional("MSIX_RELEASE")]
        private static void MsixReleaseInit(IContainerRegistry containerRegistry)
        {
            var copiedFiles = new string[]
            {
                "appdb.db", "settings.json", "shape_predictor_68_face_landmarks.dat", "dlib_face_recognition_resnet_model_v1.dat",
                "shape_predictor_5_face_landmarks.dat", "haarcascade_frontalface_default.xml", "haarcascade_profileface.xml",
                "mmod_human_face_detector.dat"
            };
            var destinationPath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\WTM\\";

            var location = Assembly.GetExecutingAssembly().Location;
            var appDir = $"{location.Substring(0, location.LastIndexOf("\\"))}\\";


            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            foreach (var file in copiedFiles)
            {
                if (!File.Exists(destinationPath + file))
                {
                    using var f = File.OpenRead(appDir + file);
                    using var d = File.Open(destinationPath + file, FileMode.Create);
                    f.CopyTo(d);
                }

#if DEV_MODE
                if (file == "settings.json" || file == "appdb.db")
                {
                    using var f = File.OpenRead(appDir + file);
                    using var d = File.Open(destinationPath + file, FileMode.Create);
                    f.CopyTo(d);
                }
#endif
            }




            var configServie = new ConfigurationService($"{destinationPath}settings.json");
            containerRegistry.RegisterInstance(typeof(IConfigurationService),
                configServie);

            var settings = configServie.Get<SqliteSettings>("sqlite");
            var cs = settings.ConnectionString;
            settings.ConnectionString = $"DataSource={destinationPath}appdb.db{cs.Substring(cs.IndexOf(';'))}";
            configServie.RegisterCustomInstance("sqlite", () => settings);


            var hcSettings = configServie.Get<HcFaceDetectionSettings>("faceDetection");
            hcSettings.ProfileModel = destinationPath + hcSettings.ProfileModel;
            hcSettings.FrontalModel = destinationPath + hcSettings.FrontalModel;
            configServie.RegisterCustomInstance("faceDetection", () => hcSettings);


            var recogSettings = configServie.Get<FaceRecognitionModelSettings>("faceRecognition");
            recogSettings.Location = destinationPath;
            configServie.RegisterCustomInstance("faceRecognition", () => recogSettings);
        }

        public static void Init(IContainerRegistry containerRegistry)
        {
            MsixReleaseInit(containerRegistry);

            
        }

        public static void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.Resolve<IEventAggregator>();
            ea.GetEvent<AppStartedEvent>().Subscribe(async window =>
            {
                ea.GetEvent<SplashScreenMsgEvent>().Publish("Loading model...");
                SharedFaceRecognitionModel.Init(containerProvider.Resolve<IConfigurationService>());
                await SharedFaceRecognitionModel.LoadTask;
                ea.GetEvent<HideSplashScreenEvent>().Publish();
            }, true);
        }
    }
}