using VeeamTestTask;
using VeeamTestTask.Synchronizer;

var config = Configuration.Load(args);

var synchronizer = new SynchronizerManager(config);
await synchronizer.Start();