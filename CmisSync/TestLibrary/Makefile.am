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
    MoqExtensions/MoqExtensions.cs \
	AuthenticationProviderTests/PersistentAuthenticationProviderTest.cs \
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
	EventsTests/EventsFilterTests/IgnoredFoldersFilterTest.cs \
	EventsTests/EventsFilterTests/IgnoreFileNamesFilterTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/EventTypesTest.cs \
	EventsTests/ExceptionEventTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/PermissionDeniedEventHandlerTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	FSDeletionHandlerTest.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/DotCMISTests.cs \
	IntegrationTests/ContentChangeIT.cs \
	IntegrationTests/IntegrationTestUtils.cs \
	LocalFilesystemActivityGenerator.cs \
	Properties/AssemblyInfo.cs \
	StorageTests/PersistentStorageTest.cs \
	StreamTests/BandwidthLimitedStreamTest.cs \
	StreamTests/ChunkedStreamTest.cs \
	StreamTests/NonClosingHashStreamTest.cs \
	StreamTests/OffsetStreamTest.cs \
	StreamTests/ProgressStreamTest.cs \
	SyncStrategiesTests/ContentChangesTest.cs \
	SyncStrategiesTests/ContentChangeEventAccumulatorTest.cs \
	SyncStrategiesTests/ContentChangeEventTransformerTest.cs \
	SyncStrategiesTests/CrawlerTest.cs \
	SyncStrategiesTests/FileSynchronizerTest.cs \
	SyncStrategiesTests/FolderSynchronizerTest.cs \
	SyncStrategiesTests/MacWatcherTest.cs \
	SyncStrategiesTests/NetWatcherTest.cs \
	SyncStrategiesTests/SingleStepEventQueue.cs \
	SyncStrategiesTests/SingleStepEventQueueTest.cs \
	SyncStrategiesTests/SituationDetectionTests/LocalSituationDetectionTest.cs \
	SyncStrategiesTests/SituationDetectionTests/RemoteSituationDetectionTest.cs \
	SyncStrategiesTests/SyncMechanismTest.cs \
	SyncStrategiesTests/SolverTests/LocalObjectAddedTest.cs \
	SyncStrategiesTests/SolverTests/RemoteObjectAddedTest.cs \
	SyncStrategiesTests/WatcherTest.cs \
	SyncStrategiesTests/WatcherTestUtils.cs \
	ContentTasksTests/ChunkedDownloaderTest.cs \
	ContentTasksTests/SimpleFileDownloaderTest.cs \
	ContentTasksTests/SimpleFileUploaderTest.cs \
	ContentTasksTests/ChunkedFileUploaderTest.cs \
	ContentTasksTests/ContentTaskUtilsTest.cs \
	UtilsTests/HttpProxyUtilsTest.cs \
	UtilsTests/CmisSyncLibUtilsTest.cs \
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
