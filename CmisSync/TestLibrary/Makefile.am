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
	ProducerTests/ContentChangeTests/ContentChangeEventAccumulatorTest.cs \
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
	FileTransmissionTests/ChunkedDownloaderTest.cs \
	FileTransmissionTests/ChunkedFileUploaderTest.cs \
	FileTransmissionTests/ContentTaskUtilsTest.cs \
	FileTransmissionTests/SimpleFileDownloaderTest.cs \
	FileTransmissionTests/SimpleFileUploaderTest.cs \
	StorageTests/DataBaseTests/EntitiesTests/MappedObjectsTest.cs \
	ProducerTests/CrawlerTests/ObjectTreeTest.cs \
	PathMatcherTests/PathMatcherTest.cs \
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
	StorageTests/DataBaseTests/MetaDataStorageTest.cs \
	StorageTests/DataBaseTests/PersistentCookieStorageTest.cs \
	StorageTests/Utf8FileNameNormalizationTest.cs \
	StreamsTests/BandwidthLimitedStreamTest.cs \
	StreamsTests/ChunkedStreamTest.cs \
	StreamsTests/NonClosingHashStreamTest.cs \
	StreamsTests/OffsetStreamTest.cs \
	StreamsTests/ProgressStreamTest.cs \
	ProducerTests/ContentChangeTests/ContentChangeEventTransformerTest.cs \
	ProducerTests/ContentChangeTests/ContentChangesTest.cs \
	ProducerTests/WatcherTests/CreatedChangedDeletedFileSystemEventHandlerTest.cs \
	ProducerTests/CrawlerTests/DescendantsCrawlerTest.cs \
	ProducerTests/WatcherTests/MacWatcherTest.cs \
	ProducerTests/WatcherTests/NetWatcherTest.cs \
	AccumulatorTests/RemoteObjectMovedOrRenamedAccumulatorTest.cs \
	ProducerTests/WatcherTests/RenamedFileSystemEventHandlerTest.cs \
	ConsumerTests/LocalSituationDetectionTest.cs \
	ConsumerTests/RemoteSituationDetectionTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectAddedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectChangedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectDeletedRemoteObjectDeletedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectDeletedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectMovedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectRenamedRemoteObjectRenamedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectRenamedTest.cs \
	ConsumerTests/SituationSolverTests/RemoteObjectAddedTest.cs \
	ConsumerTests/SituationSolverTests/RemoteObjectChangedTest.cs \
	ConsumerTests/SituationSolverTests/RemoteObjectDeletedTest.cs \
	ConsumerTests/SituationSolverTests/RemoteObjectMovedTest.cs \
	ConsumerTests/SituationSolverTests/RemoteObjectRenamedTest.cs \
	ConsumerTests/SyncMechanismTest.cs \
	ProducerTests/WatcherTests/WatcherTest.cs \
	ProducerTests/WatcherTests/WatcherTestUtils.cs \
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
