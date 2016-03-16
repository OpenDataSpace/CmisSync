SUBDIRS = Pixmaps

ASSEMBLY = DataSpaceSync
TARGET = exe

ASSEMBLY_INFO_SOURCE = Defines.cs

LIBS = $(REF_CMISSYNC) $(LOG4NET_LIBS) $(NOTIFY_SHARP_LIBS) $(DATASPACE_LIBS)

SOURCES =  \
	../AboutController.cs \
	../CertPolicyHandler.cs \
	../ControllerBase.cs \
	../EditController.cs \
	../Program.cs \
	../Properties/Resources.Designer.cs \
	../SettingController.cs \
	../SetupController.cs \
	../StatusIconController.cs \
	../TransmissionController.cs \
	../Windows/FolderTreeMVC/IgnoredFolderLoader.cs \
	../Windows/FolderTreeMVC/LoadingStatusModel.cs \
	../Windows/FolderTreeMVC/LocalFolderLoader.cs \
	../Windows/FolderTreeMVC/NodeLoader.cs \
	../Windows/FolderTreeMVC/NodeModel.cs \
	../Windows/FolderTreeMVC/NodeModelUtils.cs \
	About.cs \
	BandwidthLimitWidget.cs \
	BandwidthSettingsWidget.cs \
	CertPolicyWindow.cs \
	ClientBrand.cs \
	CmisTree/CmisTreeStore.cs \
	CmisTree/StatusCellRenderer.cs \
	Controller.cs \
	CredentialsWidget.cs \
	Edit.cs \
	Notification/NotificationUtils.cs \
	ProxyWidget.cs \
	RepositoryMenuItem.cs \
	Setting.cs \
	Setup.cs \
	SetupWindow.cs \
	Spinner.cs \
	StatusIcon.cs \
	TransmissionWidget.cs \
	TransmissionWindow.cs \
	UI.cs \
	UIHelpers.cs \
	UrlWidget.cs \
	gtk-gui/CmisSync.CredentialsWidget.cs \
	gtk-gui/CmisSync.Setting.cs \
	gtk-gui/CmisSync.TransmissionWindow.cs \
	gtk-gui/CmisSync.Widgets.BandwidthLimitWidget.cs \
	gtk-gui/CmisSync.Widgets.BandwidthSettingsWidget.cs \
	gtk-gui/CmisSync.Widgets.ProxyWidget.cs \
	gtk-gui/CmisSync.Widgets.TransmissionWidget.cs \
	gtk-gui/CmisSync.Widgets.UrlWidget.cs \
	gtk-gui/generated.cs \
	Utils.cs

RESOURCES = $(RESOURCES_RESOURCES)

RESOURCES_RESX = $(wildcard $(srcdir)/../Properties/*.resx)
RESOURCES_RESOURCES = $(addprefix CmisSync.,$(patsubst %.resx,%.resources,$(notdir $(RESOURCES_RESX))))

CmisSync.%.resources: $(srcdir)/../Properties/%.resx
	resgen2 $< $@

bin_SCRIPTS = dataspacesync

Applicationsdir = $(datadir)/applications
dist_Applications_DATA = dataspacesync.desktop dataspacesync-logview.desktop

EXTRA_DIST = README.md

include $(top_srcdir)/build/build.mk

CLEANFILES += $(RESOURCES_RESOURCES)
