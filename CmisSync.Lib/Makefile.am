ASSEMBLY = CmisSync.Lib
TARGET = library

ASSEMBLY_INFO_SOURCE = Defines.cs

LIBS = $(LOG4NET_LIBS) $(DATALIBS) $(JSON_LIBS) $(DOTCMISLIBS) $(WEBLIBS) $(MONOPOSIX)

SOURCES =  \
	ActivityListenerAggregator.cs \
	Backend.cs \
	ChunkedStream.cs \
	Cmis/CmisPermissionDeniedException.cs \
	Cmis/CmisServerNotFoundException.cs \
	Cmis/CmisUtils.cs \
	Cmis/Crypto.cs \
	Cmis/Database.cs \
	Cmis/IDatabase.cs \
	Cmis/MimeType.cs \
	Config.cs \
	ConfigManager.cs \
	ConfigMigration.cs \
	Credentials.cs \
	Defines.cs \
	Events/ActiveActivitiesManager.cs \
	Events/BubbledEvent.cs \
	Events/ConfigChangedEvent.cs \
	Events/DebugLoggingHandler.cs \
	Events/FileConflictEvent.cs \
	Events/FileTransmissionEvent.cs \
	Events/FSDeletionHandler.cs \
	Events/FSEvent.cs \
	Events/GenericSyncEventHandler.cs \
	Events/ISyncEvent.cs \
	Events/RecentChangedEvent.cs \
	Events/SyncEventHandler.cs \
	Events/SyncEventManager.cs \
	Events/SyncEventQueue.cs \
	Fetcher.cs \
	IActivityListener.cs \
	LoggingStream.cs \
	RepoBase.cs \
	RepoInfo.cs \
	Streams/BandwidthLimitedStream.cs \
	Streams/ProgressStream.cs \
	Sync/ChangeLogStrategy.cs \
	Sync/CmisRepo.cs \
	Sync/CrawlStrategy.cs \
	Sync/SynchronizedFolder.cs \
	Sync/WatcherStrategy.cs \
	Tasks/ChunkedDownloader.cs \
	Tasks/ChunkedUploader.cs \
	Tasks/FileDownloader.cs \
	Tasks/FileUploader.cs \
	Tasks/SimpleFileDownloader.cs \
	Tasks/SimpleFileUploader.cs \
	Utils.cs \
	Watcher.cs

EXTRA_DIST = CmisSync.Lib.csproj StaticDefines.cs

include $(top_srcdir)/build/build.mk
