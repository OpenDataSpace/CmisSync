ASSEMBLY = TestLibrary
TARGET = library

ASSEMBLY_INFO_SOURCE =  \
	Properties/AssemblyInfo.cs

LIBS = $(LOG4NET_LIBS) $(DATALIBS) $(JSON_LIBS) $(DOTCMISLIBS) \
	   $(LIBS_CMISSYNCLIB_DEPS) \
	   $(NUNIT_LIBS) \
	   -r:$(srcdir)/lib/Moq.dll \
	   -r:$(srcdir)/lib/xunit.dll \
	   -r:$(srcdir)/lib/xunit.extensions.dll

SOURCES =  \
    MoqExtensions/MoqExtensions.cs \
	AuthenticationProviderTests/PersistentAuthenticationProviderTest.cs \
	AuthenticationProviderTests/KerberosAuthenticationProviderTest.cs \
	DataTests/PathMatcherTest.cs \
	DataTests/MappedObjectsTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoredFoldersFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreFileNamesFilterTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/EventTypesTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/EncapsuledEventTest.cs \
	EventsTests/PermissionDeniedEventHandlerTest.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	StreamTests/BandwidthLimitedStreamTest.cs \
	StreamTests/ChunkedStreamTest.cs \
	StreamTests/OffsetStreamTest.cs \
	StreamTests/ProgressStreamTest.cs \
	StreamTests/NonClosingHashStreamTest.cs \
	StorageTests/PersistentStorageTest.cs \
	StorageTests/FileSystemWrapperTests.cs \
	SyncStrategiesTests/FileSynchronizerTest.cs \
	SyncStrategiesTests/CrawlerTest.cs \
	SyncStrategiesTests/WatcherTest.cs \
	SyncStrategiesTests/ContentChangesTest.cs \
	SyncStrategiesTests/ContentChangeEventAccumulatorTest.cs \
	SyncStrategiesTests/ContentChangeEventTransformerTest.cs \
	SyncStrategiesTests/SituationDetectionTests/LocalSituationDetectionTest.cs \
	SyncStrategiesTests/SituationDetectionTests/RemoteSituationDetectionTest.cs \
	SyncStrategiesTests/SyncMechanismTest.cs \
	SyncStrategiesTests/SingleStepEventQueue.cs \
	SyncStrategiesTests/SingleStepEventQueueTest.cs \
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
	ContentTasksTests/ChunkedDownloaderTest.cs \
	ContentTasksTests/SimpleFileDownloaderTest.cs \
	ContentTasksTests/SimpleFileUploaderTest.cs \
	ContentTasksTests/ChunkedFileUploaderTest.cs \
	ContentTasksTests/ContentTaskUtilsTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs \
	UtilsTests/CmisSyncLibUtilsTest.cs \
	UtilsTests/CredentialsTest.cs \
	ActiveActivitiesManagerTest.cs \
	CmisDatabaseTest.cs \
	ConfigurationTest.cs \
	DebugLoggingHandlerTest.cs \
	EncodingTest.cs \
	FSDeletionHandlerTest.cs \
	LocalFilesystemActivityGenerator.cs \
	WatcherTest.cs \
	TestUtils/MockUtil.cs \
	TestUtils/ObservableHandler.cs \
	TestUtils/ObservableHandlerTest.cs 
	

EXTRA_DIST = TestLibrary.csproj \
	lib/Moq.dll \
	lib/xunit.dll \
	lib/xunit.extensions.dll

include $(top_srcdir)/build/build.mk
