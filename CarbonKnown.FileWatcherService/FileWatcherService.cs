using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using CarbonKnown.FileReaders;
using CarbonKnown.FileReaders.Constants;
using CarbonKnown.FileReaders.FileHandler;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace CarbonKnown.FileWatcherService
{
    public partial class FileWatcherService : ServiceBase
    {
        private readonly IFolderMonitorFactory folderMonitorFactory;
        private readonly IHandlerFactory handlerFactory;
        private readonly Queue<FolderMonitor> services;

        public FileWatcherService(IFolderMonitorFactory folderMonitorFactory, IHandlerFactory handlerFactory)
        {
            this.folderMonitorFactory = folderMonitorFactory;
            this.handlerFactory = handlerFactory;
            services = new Queue<FolderMonitor>();
            InitializeComponent();
        }

        public void Start()
        {
            var section = FileWatcherConfigSection.Instance;
            if (section == null) return;
            var fileHandlerTypes = HandlerFactory.HandlerTypes();
            foreach (GroupInstance groupInstance in section.GroupInstances)
            {
                var basefolder = groupInstance.BaseFolder;
                if (!Directory.Exists(basefolder))
                {
                    Directory.CreateDirectory(basefolder);
                }
                foreach (var handlerType in fileHandlerTypes)
                {
                    var handlerPath = Path.Combine(basefolder, handlerType.Key);
                    if (!Directory.Exists(handlerPath))
                    {
                        Directory.CreateDirectory(handlerPath);
                    }
                    var handler = handlerFactory
                        .CreateHandler(handlerType.Key, groupInstance.Host);

                    var folderMonitor = folderMonitorFactory.CreateFolderMonitor(handlerPath, handler);

                    folderMonitor.StartMonitoring();
                    services.Enqueue(folderMonitor);
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
            while (services.Count > 0)
            {
                var service = services.Dequeue();
                try
                {
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, PolicyName.Default);
                }
            }
        }
    }
}