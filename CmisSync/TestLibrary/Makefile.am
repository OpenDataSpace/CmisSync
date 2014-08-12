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
	AccumulatorTests/LocalObjectFetcherTest.cs \
	AccumulatorTests/RemoteObjectFetcherTest.cs \
	AccumulatorTests/RemoteObjectMovedOrRenamedAccumulatorTest.cs \
	ActiveActivitiesManagerTest.cs \
	CmisTests/AuthenticationTests/AuthProviderFactoryTest.cs \
	CmisTests/AuthenticationTests/KerberosAuthenticationProviderTest.cs \
	CmisTests/AuthenticationTests/PersistentStandardAuthenticationProviderTest.cs \
	CmisTests/ConvenienceExtendersTests/CmisConvenienceExtendersTest.cs \
	CmisTests/OperationContextFactoryTest.cs \
	CmisTests/UiUtilsTests/CmisUtilsTest.cs \
	ConfigTests/ConfigurationTest.cs \
	ConfigTests/CredentialsTest.cs \
	ConfigTests/RepoInfoTests.cs \
	ConsumerTests/LocalSituationDetectionTest.cs \
	ConsumerTests/RemoteSituationDetectionTest.cs \
	ConsumerTests/SituationSolverTests/AbstractEnhancedSolverTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectAddedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectChangedTest.cs \
	ConsumerTests/SituationSolverTests/LocalObjectChangedRemoteObjectChangedTest.cs \
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
	EventsTests/EncapsuledEventTest.cs \
	EventsTests/EventHandlerPrioritiesTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/FileEventTest.cs \
	EventsTests/FileOrFolderEventFactoryTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/FsEventTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/ProxyAuthRequiredEventTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	FileTransmissionTests/ChunkedDownloaderTest.cs \
	FileTransmissionTests/ChunkedFileUploaderTest.cs \
	FileTransmissionTests/ContentTaskUtilsTest.cs \
	FileTransmissionTests/SimpleFileDownloaderTest.cs \
	FileTransmissionTests/SimpleFileUploaderTest.cs \
	FilterTests/FilterAggregatorTest.cs \
	FilterTests/GenericHandleDublicatedEventsFilterTest.cs \
	FilterTests/IgnoreAlreadyHandledContentChangeEventsFilterTest.cs \
	FilterTests/IgnoreAlreadyHandledFsEventsFilterTest.cs \
	FilterTests/IgnoreFileNamesFilterTest.cs \
	FilterTests/IgnoredFolderNamesFilterTest.cs \
	FilterTests/IgnoredFoldersFilterTest.cs \
	FilterTests/InvalidFolderNameFilterTest.cs \
	IntegrationTests/AllHandlersIT.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/FileSystemEventOrderTest.cs \
	IntegrationTests/FullRepoTests.cs \
	IntegrationTests/HttpProxyConnectionTests.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	MoqExtensions/MoqExtensions.cs \
	PathMatcherTests/PathMatcherTest.cs \
	ProducerTests/ContentChangeTests/ContentChangeEventAccumulatorTest.cs \
	ProducerTests/ContentChangeTests/ContentChangeEventTransformerTest.cs \
	ProducerTests/ContentChangeTests/ContentChangesTest.cs \
	ProducerTests/CrawlerTests/DescendantsCrawlerTest.cs \
	ProducerTests/CrawlerTests/ObjectTreeTest.cs \
	ProducerTests/WatcherTests/CreatedChangedDeletedFileSystemEventHandlerTest.cs \
	ProducerTests/WatcherTests/MacWatcherTest.cs \
	ProducerTests/WatcherTests/NetWatcherTest.cs \
	ProducerTests/WatcherTests/RenamedFileSystemEventHandlerTest.cs \
	ProducerTests/WatcherTests/WatcherTest.cs \
	ProducerTests/WatcherTests/WatcherTestUtils.cs \
	QueueingTests/DebugLoggingHandlerTest.cs \
	QueueingTests/DelayRetryAndNextSyncEventHandlerTest.cs \
	QueueingTests/SyncEventManagerTest.cs \
	QueueingTests/SyncEventQueueTest.cs \
	EventManagerInitializerTest.cs \
	StorageTests/DataBaseTests/DBreezeTests.cs \
	StorageTests/DataBaseTests/EntitiesTests/MappedObjectsTest.cs \
	StorageTests/DataBaseTests/EntitiesTests/IFileConvenienceExtendersTest.cs \
	StorageTests/DataBaseTests/MetaDataStorageTest.cs \
	StorageTests/DataBaseTests/PersistentCookieStorageTest.cs \
	StorageTests/FileSystemTests/ExtendedAttributeReaderDosTest.cs \
	StorageTests/FileSystemTests/ExtendedAttributeReaderUnixTest.cs \
	StorageTests/FileSystemTests/FileSystemWrapperTests.cs \
	StorageTests/FileSystemTests/Utf8FileNameNormalizationTest.cs \
	StreamsTests/BandwidthLimitedStreamTest.cs \
	StreamsTests/ChunkedStreamTest.cs \
	StreamsTests/NonClosingHashStreamTest.cs \
	StreamsTests/OffsetStreamTest.cs \
	StreamsTests/ProgressStreamTest.cs \
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
	UtilsTests/EncodingTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs

EXTRA_DIST = TestLibrary.csproj \
	$(MOQ_PATH)

include $(top_srcdir)/build/build.mk
