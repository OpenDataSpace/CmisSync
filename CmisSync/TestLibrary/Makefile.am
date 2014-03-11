ASSEMBLY = TestLibrary
TARGET = library

ASSEMBLY_INFO_SOURCE =  \
	Properties/AssemblyInfo.cs

LIBS = $(LOG4NET_LIBS) $(DATALIBS) $(JSON_LIBS) $(DOTCMISLIBS) $(MONOMAC_LIBS) \
	   $(LIBS_CMISSYNCLIB_DEPS) \
	   $(NUNIT_LIBS) \
	   -r:$(srcdir)/lib/Moq.dll \
	   -r:$(srcdir)/lib/xunit.dll \
	   -r:$(srcdir)/lib/xunit.extensions.dll

SOURCES =  \
	ActiveActivitiesManagerTest.cs \
	AuthenticationProviderTests/KerberosAuthenticationProviderTest.cs \
	AuthenticationProviderTests/PersistentAuthenticationProviderTest.cs \
	CmisDatabaseTest.cs \
	ConfigurationTest.cs \
	ContentTasksTests/ChunkedDownloaderTest.cs \
	ContentTasksTests/ChunkedFileUploaderTest.cs \
	ContentTasksTests/ContentTaskUtilsTest.cs \
	ContentTasksTests/SimpleFileDownloaderTest.cs \
	ContentTasksTests/SimpleFileUploaderTest.cs \
	DataTests/MappedObjectsTest.cs \
	DataTests/PathMatcherTest.cs \
	DebugLoggingHandlerTest.cs \
	EncodingTest.cs \
	EventsTests/EncapsuledEventTest.cs \
	EventsTests/EventTypesTest.cs \
	EventsTests/EventsFilterTests/IgnoreFileNamesFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFoldersFilterTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/FileEventTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/PermissionDeniedEventHandlerTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	IntegrationTests/AllHandlersIT.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	LocalFilesystemActivityGenerator.cs \
	MoqExtensions/MoqExtensions.cs \
	Properties/AssemblyInfo.cs \
	StorageTests/FileSystemWrapperTests.cs \
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
	SyncStrategiesTests/FileSynchronizerTest.cs \
	SyncStrategiesTests/FolderSynchronizerTest.cs \
	SyncStrategiesTests/MacWatcherTest.cs \
	SyncStrategiesTests/NetWatcherTest.cs \
	SyncStrategiesTests/SingleStepEventQueue.cs \
	SyncStrategiesTests/SingleStepEventQueueTest.cs \
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
	TestUtils/MockUtil.cs \
	TestUtils/ObservableHandler.cs \
	TestUtils/ObservableHandlerTest.cs \
	UtilsTests/CmisSyncLibUtilsTest.cs \
	UtilsTests/CredentialsTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs \
	WatcherTest.cs \
    MoqExtensions/MoqExtensions.cs 
	

EXTRA_DIST = TestLibrary.csproj \
	lib/Moq.dll \
	lib/xunit.dll \
	lib/xunit.extensions.dll

include $(top_srcdir)/build/build.mk
