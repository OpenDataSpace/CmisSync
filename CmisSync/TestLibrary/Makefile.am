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
	DataTests/PathMatcherTest.cs \
	DataTests/SyncObjectsTest.cs \
	EventsTests/EventsFilterTests/InvalidFolderNameFilterTest.cs \
	EventsTests/EventTypesTest.cs \
	EventsTests/FileTransmissionEventTest.cs \
	EventsTests/SyncEventManagerTest.cs \
	EventsTests/SyncEventQueueTest.cs \
	EventsTests/GenericSyncEventHandlerTest.cs \
	EventsTests/FullSyncCompletedEventTest.cs \
	EventsTests/StartNextSyncEventTest.cs \
	EventsTests/EncapsuledEventTest.cs \
	IntegrationTests/CmisSyncTests.cs \
	IntegrationTests/DotCMISTests.cs \
	StreamTests/BandwidthLimitedStreamTest.cs \
	StreamTests/ChunkedStreamTest.cs \
	StreamTests/OffsetStreamTest.cs \
	StreamTests/ProgressStreamTest.cs \
	StreamTests/NonClosingHashStreamTest.cs \
	SyncStrategiesTests/FileSynchronizerTest.cs \
	SyncStrategiesTests/CrawlerTest.cs \
	SyncStrategiesTests/WatcherTest.cs \
	SyncStrategiesTests/ContentChangesTest.cs \
	TasksTests/ChunkedDownloaderTest.cs \
	TasksTests/SimpleFileDownloaderTest.cs \
	TasksTests/SimpleFileUploaderTest.cs \
	TasksTests/ChunkedFileUploaderTest.cs \
	ActiveActivitiesManagerTest.cs \
	CmisDatabaseTest.cs \
	CmisSyncLibUtilsTest.cs \
	ConfigurationTest.cs \
	DebugLoggingHandlerTest.cs \
	EncodingTest.cs \
	FSDeletionHandlerTest.cs \
	LocalFilesystemActivityGenerator.cs \
	WatcherTest.cs
	

EXTRA_DIST = TestLibrary.csproj \
	lib/Moq.dll \
	lib/xunit.dll \
	lib/xunit.extensions.dll

include $(top_srcdir)/build/build.mk
