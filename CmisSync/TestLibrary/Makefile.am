ASSEMBLY = TestLibrary
TARGET = library

ASSEMBLY_INFO_SOURCE =  \
	Properties/AssemblyInfo.cs

LIBS = $(LOG4NET_LIBS) $(DATALIBS) $(JSON_LIBS) $(DOTCMIS_LIBS) $(MONOMAC_LIBS) \
	$(LIBS_CMISSYNCLIB_DEPS) \
	$(NUNIT_LIBS) \
	$(DBREEZE_LIBS) \
	$(MOQ_LIBS)

SOURCES =  \
	AccumulatorTests/ContentChangeEventAccumulatorTest.cs \
	AccumulatorTests/LocalObjectFetcherTest.cs \
	AccumulatorTests/RemoteObjectFetcherTest.cs \
	ActiveActivitiesManagerTest.cs \
	CmisTests/AuthenticationTests/AuthProviderFactoryTest.cs \
	CmisTests/AuthenticationTests/KerberosAuthenticationProviderTest.cs \
	CmisTests/AuthenticationTests/PersistentStandardAuthenticationProviderTest.cs \
	CmisTests/OperationContextFactoryTest.cs \
	CmisTests/UiUtilsTests/CmisUtilsTest.cs \
	ConfigTests/ConfigurationTest.cs \
	ConfigTests/CredentialsTest.cs \
	ConfigTests/RepoInfoTests.cs \
	ContentTasksTests/ChunkedDownloaderTest.cs \
	ContentTasksTests/ChunkedFileUploaderTest.cs \
	ContentTasksTests/ContentTaskUtilsTest.cs \
	ContentTasksTests/SimpleFileDownloaderTest.cs \
	ContentTasksTests/SimpleFileUploaderTest.cs \
	DataTests/MappedObjectsTest.cs \
	DataTests/ObjectTreeTest.cs \
	DataTests/PathMatcherTest.cs \
	UtilsTests/EncodingTest.cs \
	EventsTests/DebugLoggingHandlerTest.cs \
	EventsTests/EncapsuledEventTest.cs \
	EventsTests/EventHandlerPrioritiesTest.cs \
	EventsTests/EventsFilterTests/FilterAggregatorTest.cs \
	EventsTests/EventsFilterTests/GenericHandleDublicatedEventsFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreAlreadyHandledContentChangeEventsFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreAlreadyHandledFsEventsFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreFileNamesFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFolderNamesFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFoldersFilterTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/FileEventTest.cs \
	EventsTests/FileOrFolderEventFactoryTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/FsEventTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/ProxyAuthRequiredEventTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	EventsTests/SyncStrategyInitializerTest.cs \
	IntegrationTests/AllHandlersIT.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/DBreezeTests.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/FileSystemEventOrderTest.cs \
	IntegrationTests/FullRepoTests.cs \
	IntegrationTests/HttpProxyConnectionTests.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	MoqExtensions/MoqExtensions.cs \
	StorageTests/ExtendedAttributeReaderDosTest.cs \
	StorageTests/ExtendedAttributeReaderUnixTest.cs \
	StorageTests/FileSystemWrapperTests.cs \
	StorageTests/MetaDataStorageTest.cs \
	StorageTests/PersistentCookieStorageTest.cs \
	StorageTests/PersistentStorageTest.cs \
	StorageTests/Utf8FileNameNormalizationTest.cs \
	StreamTests/BandwidthLimitedStreamTest.cs \
	StreamTests/ChunkedStreamTest.cs \
	StreamTests/NonClosingHashStreamTest.cs \
	StreamTests/OffsetStreamTest.cs \
	StreamTests/ProgressStreamTest.cs \
	SyncStrategiesTests/ContentChangeEventTransformerTest.cs \
	SyncStrategiesTests/ContentChangesTest.cs \
	SyncStrategiesTests/CreatedChangedDeletedFileSystemEventHandlerTest.cs \
	SyncStrategiesTests/DescendantsCrawlerTest.cs \
	SyncStrategiesTests/MacWatcherTest.cs \
	SyncStrategiesTests/NetWatcherTest.cs \
	SyncStrategiesTests/RemoteObjectMovedOrRenamedAccumulatorTest.cs \
	SyncStrategiesTests/RenamedFileSystemEventHandlerTest.cs \
	ConsumerTests/LocalSituationDetectionTest.cs \
	ConsumerTests/RemoteSituationDetectionTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectAddedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectChangedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectDeletedRemoteObjectDeletedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectDeletedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectMovedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectRenamedRemoteObjectRenamedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectRenamedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectAddedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectChangedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectDeletedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectMovedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectRenamedTest.cs \
	ConsumerTests/SyncMechanismTest.cs \
	SyncStrategiesTests/WatcherTest.cs \
	SyncStrategiesTests/WatcherTestUtils.cs \
	TestUtils/CmisConvenienceExtenders.cs \
	TestUtils/IsTestWithConfiguredLog4Net.cs \
	TestUtils/MockMetaDataStorageUtil.cs \
	TestUtils/MockOfIDocumentUtil.cs \
	TestUtils/MockOfIFileSystemInfoFactoryUtil.cs \
	TestUtils/MockOfIFilterAggregatorUtil.cs \
	TestUtils/MockOfIFolderUtil.cs \
	TestUtils/MockOfISyncEventQueue.cs \
	TestUtils/MockSessionUtil.cs \
	TestUtils/ObservableHandler.cs \
	TestUtils/ObservableHandlerTest.cs \
	TestUtils/SingleStepEventQueue.cs \
	TestUtils/SingleStepEventQueueTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs

EXTRA_DIST = TestLibrary.csproj \
	$(MOQ_PATH)

include $(top_srcdir)/build/build.mk
