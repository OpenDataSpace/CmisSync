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
	Events/AbstractEventProducer.cs \
	Events/ActiveActivitiesManager.cs \
	Events/BubbledEvent.cs \
	Events/ChangeEnums.cs \
	Events/ConfigChangedEvent.cs \
	Events/CrawlRequestEvent.cs \
	Events/DebugLoggingHandler.cs \
	Events/EncapsuledEvent.cs \
	Events/EventFilter/AbstractFileFilter.cs \
	Events/EventFilter/FailedOperationsFilter.cs \
	Events/EventFilter/IgnoredFileNamesFilter.cs \
	Events/EventFilter/IgnoredFilesFilter.cs \
	Events/EventFilter/IgnoredFoldersFilter.cs \
	Events/EventFilter/InvalidFolderNameFilter.cs \
	Events/FileConflictEvent.cs \
	Events/FileDownloadRequest.cs \
	Events/FileEvent.cs \
	Events/FileTransmissionEvent.cs \
	Events/FolderEvent.cs \
	Events/FSDeletionHandler.cs \
	Events/FSEvent.cs \
	Events/FullSyncCompletedEvent.cs \
	Events/GenericSyncEventHandler.cs \
	Events/ISyncEvent.cs \
	Events/RecentChangedEvent.cs \
	Events/RemoteEvent.cs \
	Events/RequestIgnoredEvent.cs \
	Events/StartNextSyncEvent.cs \
	Events/SyncEventHandler.cs \
	Events/SyncEventManager.cs \
	Events/SyncEventQueue.cs \
	Events/SyncScheduler.cs \
	Fetcher.cs \
	IActivityListener.cs \
	LoggingStream.cs \
	RepoBase.cs \
	RepoInfo.cs \
	Storage/EventStorage.cs \
	Storage/FailedOperationsStorage.cs \
	Storage/GeneralRepoStorage.cs \
	Storage/MetaDataStorage.cs \
	Storage/PersistentStorage.cs \
	Streams/BandwidthLimitedStream.cs \
	Streams/ForwardReadingStream.cs \
	Streams/NonClosingHashStream.cs \
	Streams/OffsetStream.cs \
	Streams/ProgressStream.cs \
	Streams/StreamWrapper.cs \
	Sync/ChangeLogStrategy.cs \
	Sync/CmisRepo.cs \
	Sync/CrawlStrategy.cs \
	Sync/Strategy/ContentChanges.cs \
	Sync/Strategy/Crawler.cs \
	Sync/Strategy/FileSynchronizer.cs \
	Sync/Strategy/FolderSynchronizer.cs \
	Sync/Strategy/Watcher.cs \
	Sync/SynchronizedFolder.cs \
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
