ASSEMBLY = TestLibrary
TARGET = library

ASSEMBLY_INFO_SOURCE =  \
	Properties/AssemblyInfo.cs

LIBS = $(LOG4NET_LIBS) $(DATALIBS) $(JSON_LIBS) $(DOTCMISLIBS) $(MONOMAC_LIBS) \
	$(LIBS_CMISSYNCLIB_DEPS) \
	$(NUNIT_LIBS) \
	$(DBREEZE_LIBS) \
	$(MOQ_LIBS)

SOURCES =  \
	ActiveActivitiesManagerTest.cs \
	AuthenticationProviderTests/AuthProviderFactoryTest.cs \
	AuthenticationProviderTests/KerberosAuthenticationProviderTest.cs \
	AuthenticationProviderTests/PersistentStandardAuthenticationProviderTest.cs \
	ConfigurationTest.cs \
	ContentTasksTests/ChunkedDownloaderTest.cs \
	ContentTasksTests/ChunkedFileUploaderTest.cs \
	ContentTasksTests/ContentTaskUtilsTest.cs \
	ContentTasksTests/SimpleFileDownloaderTest.cs \
	ContentTasksTests/SimpleFileUploaderTest.cs \
	DataTests/MappedObjectsTest.cs \
	DataTests/PathMatcherTest.cs \
	EncodingTest.cs \
	EventsTests/DebugLoggingHandlerTest.cs \
	EventsTests/EncapsuledEventTest.cs \
	EventsTests/EventHandlerPrioritiesTest.cs \
	EventsTests/EventsFilterTests/GenericHandleDublicatedEventsFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFolderNamesFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFoldersFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreFileNamesFilterTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/EventTypesTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/FileEventTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/ProxyAuthRequiredEventTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	IntegrationTests/AllHandlersIT.cs \
	IntegrationTests/CmisRepoTest.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/DBreezeTests.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	LocalFilesystemActivityGenerator.cs \
	MoqExtensions/MoqExtensions.cs \
	RepoInfoTests.cs \
	StorageTests/ExtendedAttributeReaderUnixTest.cs \
	StorageTests/FileSystemWrapperTests.cs \
	StorageTests/MetaDataStorageTest.cs \
	StorageTests/PersistentCookieStorageTest.cs \
	StorageTests/PersistentStorageTest.cs \
	StreamTests/BandwidthLimitedStreamTest.cs \
	StreamTests/ChunkedStreamTest.cs \
	StreamTests/NonClosingHashStreamTest.cs \
	StreamTests/OffsetStreamTest.cs \
	StreamTests/ProgressStreamTest.cs \
	SyncStrategiesTests/ContentChangeEventAccumulatorTest.cs \
	SyncStrategiesTests/ContentChangeEventTransformerTest.cs \
	SyncStrategiesTests/ContentChangesTest.cs \
	SyncStrategiesTests/CrawlerTest.cs \
	SyncStrategiesTests/LocalObjectFetcherTest.cs \
	SyncStrategiesTests/MacWatcherTest.cs \
	SyncStrategiesTests/NetWatcherTest.cs \
	SyncStrategiesTests/RemoteObjectFetcherTest.cs \
	SyncStrategiesTests/SituationDetectionTests/LocalSituationDetectionTest.cs \
	SyncStrategiesTests/SituationDetectionTests/RemoteSituationDetectionTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectAddedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectChangedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectDeletedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectMovedTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectRenamedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectAddedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectChangedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectDeletedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectMovedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectRenamedTest.cs \
	SyncStrategiesTests/SyncMechanismTest.cs \
	SyncStrategiesTests/WatcherTest.cs \
	SyncStrategiesTests/WatcherTestUtils.cs \
	TestUtils/MockMetaDataStorageUtil.cs \
	TestUtils/MockOfIFileSystemInfoFactoryUtil.cs \
	TestUtils/MockSessionUtil.cs \
	TestUtils/ObservableHandler.cs \
	TestUtils/ObservableHandlerTest.cs \
	TestUtils/SingleStepEventQueue.cs \
	TestUtils/SingleStepEventQueueTest.cs \
	UtilsTests/CmisSyncLibUtilsTest.cs \
	UtilsTests/CredentialsTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs 
	

EXTRA_DIST = TestLibrary.csproj \
	$(MOQ_PATH)

include $(top_srcdir)/build/build.mk
