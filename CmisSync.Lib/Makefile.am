ASSEMBLY = CmisSync.Lib
TARGET = library

ASSEMBLY_INFO_SOURCE = Defines.cs

LIBS = $(LOG4NET_LIBS) $(CONFIGLIBS) $(DATASPACE_LIBS) $(JSON_LIBS) $(DOTCMIS_LIBS) $(WEBLIBS) $(MONOPOSIX) $(MONOMAC_LIBS) $(DBREEZE_LIBS)

SOURCES =  \
	Accumulator/LocalObjectFetcher.cs \
	Accumulator/RemoteObjectFetcher.cs \
	Accumulator/RemoteObjectMovedOrRenamedAccumulator.cs \
	ActivityListenerAggregator.cs \
	Algorithms/CyclicDependencies/CycleDetector.cs \
	Algorithms/CyclicDependencies/ICycleDetector.cs \
	Algorithms/Tarjan/AbstractTarjanNode.cs \
	Algorithms/Tarjan/Tarjan.cs \
	Backend.cs \
	ClientBrandBase.cs \
	Cmis/AbstractNotifyingRepository.cs \
	Cmis/Authentication/AuthProviderFactory.cs \
	Cmis/Authentication/IDisposableAuthProvider.cs \
	Cmis/Authentication/PersistentNtlmAuthenticationProvider.cs \
	Cmis/Authentication/PersistentStandardAuthenticationProvider.cs \
	Cmis/Authentication/StandardAuthenticationProviderWrapper.cs \
	Cmis/ConvenienceExtenders/AllowableActionExtenders.cs \
	Cmis/ConvenienceExtenders/DocumentExtenders.cs \
	Cmis/ConvenienceExtenders/ExceptionExtenders.cs \
	Cmis/ConvenienceExtenders/FileableObjectExtenders.cs \
	Cmis/ConvenienceExtenders/FolderExtenders.cs \
	Cmis/ConvenienceExtenders/LinkExtenders.cs \
	Cmis/ConvenienceExtenders/LinkType.cs \
	Cmis/ConvenienceExtenders/ObjectExtenders.cs \
	Cmis/ConvenienceExtenders/RepositoryInfoExtenders.cs \
	Cmis/ConvenienceExtenders/SessionExtenders.cs \
	Cmis/ConvenienceExtenders/SessionFactoryExtenders.cs \
	Cmis/DotCMISLogListener.cs \
	Cmis/INotifyRepositoryPropertyChanged.cs \
	Cmis/MimeType.cs \
	Cmis/OperationContextFactory.cs \
	Cmis/Repository.cs \
	Cmis/RepositoryExceptionEventArgs.cs \
	Cmis/RepositoryStatus.cs \
	Cmis/RepositoryStatusAggregator.cs \
	Cmis/SyncStatus.cs \
	Cmis/UiUtils/CmisUtils.cs \
	Cmis/UiUtils/LoginCredentials.cs \
	Cmis/UiUtils/LoginException.cs \
	Cmis/UiUtils/LogonRepositoryInfo.cs \
	Cmis/UiUtils/UiConvenienceExtenders.cs \
	Config/AuthenticationType.cs \
	Config/Brand.cs \
	Config/CmisRepoCredentials.cs \
	Config/Config.cs \
	Config/ConfigManager.cs \
	Config/ConfigMigration.cs \
	Config/Crypto.cs \
	Config/DefaultEntries.cs \
	Config/Feature.cs \
	Config/Password.cs \
	Config/ProxySettings.cs \
	Config/RepoInfo.cs \
	Config/ServerCredentials.cs \
	Config/User.cs \
	Config/UserCredentials.cs \
	Config/XmlUri.cs \
	Consumer/ISituationDetection.cs \
	Consumer/LocalSituationDetection.cs \
	Consumer/RemoteSituationDetection.cs \
	Consumer/SituationSolver/AbstractEnhancedSolver.cs \
	Consumer/SituationSolver/FileSystemInfoConvenienceExtenders.cs \
	Consumer/SituationSolver/ISolver.cs \
	Consumer/SituationSolver/LocalObjectAdded.cs \
	Consumer/SituationSolver/LocalObjectChanged.cs \
	Consumer/SituationSolver/LocalObjectChangedRemoteObjectChanged.cs \
	Consumer/SituationSolver/LocalObjectChangedRemoteObjectMoved.cs \
	Consumer/SituationSolver/LocalObjectChangedRemoteObjectRenamed.cs \
	Consumer/SituationSolver/LocalObjectDeleted.cs \
	Consumer/SituationSolver/LocalObjectDeletedRemoteObjectChanged.cs \
	Consumer/SituationSolver/LocalObjectDeletedRemoteObjectDeleted.cs \
	Consumer/SituationSolver/LocalObjectDeletedRemoteObjectRenamedOrMoved.cs \
	Consumer/SituationSolver/LocalObjectMoved.cs \
	Consumer/SituationSolver/LocalObjectMovedRemoteObjectChanged.cs \
	Consumer/SituationSolver/LocalObjectMovedRemoteObjectMoved.cs \
	Consumer/SituationSolver/LocalObjectMovedRemoteObjectRenamed.cs \
	Consumer/SituationSolver/LocalObjectRenamed.cs \
	Consumer/SituationSolver/LocalObjectRenamedOrMovedRemoteObjectDeleted.cs \
	Consumer/SituationSolver/LocalObjectRenamedRemoteObjectChanged.cs \
	Consumer/SituationSolver/LocalObjectRenamedRemoteObjectMoved.cs \
	Consumer/SituationSolver/LocalObjectRenamedRemoteObjectRenamed.cs \
	Consumer/SituationSolver/NothingToDoSolver.cs \
	Consumer/SituationSolver/PrivateWorkingCopy/AbstractEnhancedSolverWithPWC.cs \
	Consumer/SituationSolver/PrivateWorkingCopy/LocalObjectAddedWithPWC.cs \
	Consumer/SituationSolver/PrivateWorkingCopy/LocalObjectChangedRemoteObjectChangedWithPWC.cs \
	Consumer/SituationSolver/PrivateWorkingCopy/LocalObjectChangedWithPWC.cs \
	Consumer/SituationSolver/RemoteObjectAdded.cs \
	Consumer/SituationSolver/RemoteObjectChanged.cs \
	Consumer/SituationSolver/RemoteObjectDeleted.cs \
	Consumer/SituationSolver/RemoteObjectMoved.cs \
	Consumer/SituationSolver/RemoteObjectRenamed.cs \
	Consumer/SyncMechanism.cs \
	Defines.cs \
	EventManagerInitializer.cs \
	Events/AbstractFolderEvent.cs \
	Events/BubbledEvent.cs \
	Events/ChangeEnums.cs \
	Events/CmisConnectionExceptionEvent.cs \
	Events/ConfigChangedEvent.cs \
	Events/ConfigurationNeededEvent.cs \
	Events/ContentChangeEvent.cs \
	Events/CrawlRequestEvent.cs \
	Events/EncapsuledEvent.cs \
	Events/EventCategory.cs \
	Events/ExceptionEvent.cs \
	Events/FSEvent.cs \
	Events/FSMovedEvent.cs \
	Events/FileConflictEvent.cs \
	Events/FileEvent.cs \
	Events/FileMovedEvent.cs \
	Events/FileOrFolderEventFactory.cs \
	Events/FolderEvent.cs \
	Events/FolderMovedEvent.cs \
	Events/FullSyncCompletedEvent.cs \
	Events/ICountableEvent.cs \
	Events/IFSEvent.cs \
	Events/IFSMovedEvent.cs \
	Events/IFilterableEvent.cs \
	Events/IFilterableLocalPathEvent.cs \
	Events/IFilterableNameEvent.cs \
	Events/IFilterableRemoteObjectEvent.cs \
	Events/IFilterableRemotePathEvent.cs \
	Events/IRemoveFromLoggingEvent.cs \
	Events/ISyncEvent.cs \
	Events/InteractionNeededEvent.cs \
	Events/PermissionDeniedEvent.cs \
	Events/ProxyAuthRequiredEvent.cs \
	Events/RecentChangedEvent.cs \
	Events/RemoteEvent.cs \
	Events/RepoConfigChangedEvent.cs \
	Events/RequestIgnoredEvent.cs \
	Events/StartNextSyncEvent.cs \
	Events/SuccessfulLoginEvent.cs \
	Exceptions/AbstractInteractionNeededException.cs \
	Exceptions/ExceptionLevel.cs \
	Exceptions/InteractionNeededException.cs \
	Exceptions/QuotaExceededException.cs \
	Exceptions/RetryException.cs \
	Exceptions/VirusDetectedException.cs \
	FileTransmission/ChunkedDownloader.cs \
	FileTransmission/ChunkedUploader.cs \
	FileTransmission/ContentTaskUtils.cs \
	FileTransmission/IFileDownloader.cs \
	FileTransmission/IFileUploader.cs \
	FileTransmission/SimpleFileDownloader.cs \
	FileTransmission/SimpleFileUploader.cs \
	FileTransmission/TransmissionFactory.cs \
	FileTransmission/TransmissionManager.cs \
	FileTransmission/UploadFailedException.cs \
	Filter/AbstractFileFilter.cs \
	Filter/FilterAggregator.cs \
	Filter/GenericHandleDublicatedEventsFilter.cs \
	Filter/IFilterAggregator.cs \
	Filter/IgnoreAlreadyHandledContentChangeEventsFilter.cs \
	Filter/IgnoreAlreadyHandledFsEventsFilter.cs \
	Filter/IgnoredFileNamesFilter.cs \
	Filter/IgnoredFolderNameFilter.cs \
	Filter/IgnoredFoldersFilter.cs \
	Filter/InvalidFolderNameFilter.cs \
	Filter/RegexIgnore/RegexIgnoreEventTransformer.cs \
	Filter/RegexIgnore/RegexIgnoreFilter.cs \
	Filter/ReportingFilter.cs \
	Filter/RepositoryRootDeletedDetection.cs \
	Filter/SymlinkFilter.cs \
	HashAlgorithm/IReusableHashAlgorithm.cs \
	HashAlgorithm/SHA1Reuse.cs \
	HttpProxyUtils.cs \
	IActivityListener.cs \
	PathMatcher/IPathMatcher.cs \
	PathMatcher/PathMatcher.cs \
	Producer/ContentChange/ContentChangeEventAccumulator.cs \
	Producer/ContentChange/ContentChangeEventTransformer.cs \
	Producer/ContentChange/ContentChanges.cs \
	Producer/Crawler/CrawlEventCollection.cs \
	Producer/Crawler/CrawlEventGenerator.cs \
	Producer/Crawler/CrawlEventNotifier.cs \
	Producer/Crawler/DescendantsCrawler.cs \
	Producer/Crawler/DescendantsTreeBuilder.cs \
	Producer/Crawler/DescendantsTreeCollection.cs \
	Producer/Crawler/IDescendantsTreeBuilder.cs \
	Producer/Crawler/LocalEventGenerator.cs \
	Producer/Crawler/RemoteEventGenerator.cs \
	Producer/Watcher/CreatedChangedDeletedFileSystemEventHandler.cs \
	Producer/Watcher/IWatcherProducer.cs \
	Producer/Watcher/MacWatcher.cs \
	Producer/Watcher/NetWatcher.cs \
	Producer/Watcher/RenamedFileSystemEventHandler.cs \
	Producer/Watcher/WatcherConsumer.cs \
	Queueing/AbstractEventProducer.cs \
	Queueing/ConnectionInterruptedHandler.cs \
	Queueing/ConnectionScheduler.cs \
	Queueing/DebugLoggingHandler.cs \
	Queueing/DelayRetryAndNextSyncEventHandler.cs \
	Queueing/EventHandlerPriorities.cs \
	Queueing/GenericSyncEventHandler.cs \
	Queueing/IConnectionScheduler.cs \
	Queueing/ICountingQueue.cs \
	Queueing/IDisposableSyncEventQueue.cs \
	Queueing/IEventCounter.cs \
	Queueing/ISyncEventManager.cs \
	Queueing/ISyncEventQueue.cs \
	Queueing/QueuedCategorizedEventsCounter.cs \
	Queueing/QueuedEventsCounter.cs \
	Queueing/ReportingSyncEventHandler.cs \
	Queueing/SyncEventHandler.cs \
	Queueing/SyncEventManager.cs \
	Queueing/SyncEventQueue.cs \
	Queueing/SyncScheduler.cs \
	Queueing/Unsubscriber.cs \
	SelectiveIgnore/AbstractIgnoredEntity.cs \
	SelectiveIgnore/CmisSelectiveIgnoreCapability.cs \
	SelectiveIgnore/IIgnoredEntitiesCollection.cs \
	SelectiveIgnore/IIgnoredEntitiesStorage.cs \
	SelectiveIgnore/IgnoreFlagChangeDetection.cs \
	SelectiveIgnore/IgnoredEntitiesCollection.cs \
	SelectiveIgnore/IgnoredEntitiesStorage.cs \
	SelectiveIgnore/IgnoredEntity.cs \
	SelectiveIgnore/IgnoredState.cs \
	SelectiveIgnore/SelectiveIgnoreEventTransformer.cs \
	SelectiveIgnore/SelectiveIgnoreFilter.cs \
	Status/ConnectionStatus.cs \
	Status/RepositoryStatus.cs \
	Status/SynchronizationStatus.cs \
	Storage/Database/DBreezeInitializerSingleton.cs \
	Storage/Database/DublicateGuidException.cs \
	Storage/Database/Entities/FileContentConvenienceExtenders.cs \
	Storage/Database/Entities/FileTransmissionObject.cs \
	Storage/Database/Entities/IFileTransmissionObject.cs \
	Storage/Database/Entities/IMappedObject.cs \
	Storage/Database/Entities/MappedObject.cs \
	Storage/Database/EntryNotFoundException.cs \
	Storage/Database/FileTransmissionStorage.cs \
	Storage/Database/GraphOutputConvenienceExtension.cs \
	Storage/Database/ICookieStorage.cs \
	Storage/Database/IDotTreeWriter.cs \
	Storage/Database/IFileTransmissionStorage.cs \
	Storage/Database/IMetaDataStorage.cs \
	Storage/Database/IMetaDataStorageConvenienceExtender.cs \
	Storage/Database/IObjectTree.cs \
	Storage/Database/MetaDataStorage.cs \
	Storage/Database/ObjectTree.cs \
	Storage/Database/PersistentCookieStorage.cs \
	Storage/FileSystem/DateTimeToFileConverter.cs \
	Storage/FileSystem/DirectoryInfoWrapper.cs \
	Storage/FileSystem/ExtendedAttributeException.cs \
	Storage/FileSystem/ExtendedAttributeReaderDos.cs \
	Storage/FileSystem/ExtendedAttributeReaderUnix.cs \
	Storage/FileSystem/FSType.cs \
	Storage/FileSystem/FileInfoWrapper.cs \
	Storage/FileSystem/FileSystemInfoFactory.cs \
	Storage/FileSystem/FileSystemInfoWrapper.cs \
	Storage/FileSystem/IDirectoryInfo.cs \
	Storage/FileSystem/IExtendedAttributeReader.cs \
	Storage/FileSystem/IFileInfo.cs \
	Storage/FileSystem/IFileSystemInfo.cs \
	Storage/FileSystem/IFileSystemInfoFactory.cs \
	Storage/FileSystem/IFileSystemWatcher.cs \
	Storage/FileSystem/ReadOnlyIgnoringDirectoryInfoDecorator.cs \
	Storage/FileSystem/ReadOnlyIgnoringFileInfoDecorator.cs \
	Storage/FileSystem/ReadOnlyIgnoringFileSystemInfoDecorator.cs \
	Storage/FileSystem/RestoreModificationDateException.cs \
	Utils.cs

EXTRA_DIST = CmisSync.Lib.csproj StaticDefines.cs

include $(top_srcdir)/build/build.mk
