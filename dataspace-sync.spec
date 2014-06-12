Name:		dataspace-sync
Version:	1.3.0
Release:	1%{?dist}
Summary:	DataSpace Sync Client

Group:		Applications/Internet
License:	GPL
URL:		https://graudata.com
Source0:	%{name}-%{version}.tar.gz

BuildRequires: autoconf, automake
BuildRequires: mono-devel
BuildRequires: gtk-sharp2-devel
BuildRequires: dbus-sharp-devel dbus-sharp-glib-devel notify-sharp-devel

%description
DataSpace Sync allows you to keep in sync with your DataSpace (CMIS) repository.
See: https://graudata.com


%prep
%setup -q


%build
aclocal -I build/m4/shamrock -I build/m4/cmissync
automake -a
%configure --with-newtonsoft-json=Extras/Newtonsoft.Json.dll --with-dotcmis=Extras/DotCMIS.dll --with-log4net=Extras/log4net.dll --with-nunit=Extras/nunit.framework.dll --enable-appindicator=no --enable-fat-binary=no
make %{?_smp_mflags}


%install
make install DESTDIR=%{buildroot}
cp Extras/Newtonsoft.Json.dll %{buildroot}/%{_libdir}/dataspace-sync
cp Extras/DotCMIS.dll %{buildroot}/%{_libdir}/dataspace-sync
cp Extras/log4net.dll %{buildroot}/%{_libdir}/dataspace-sync
cp Extras/nunit.framework.dll %{buildroot}/%{_libdir}/dataspace-sync
cp CmisSync/TestLibrary/lib/Moq.dll %{buildroot}/%{_libdir}/dataspace-sync
cp bin/CmisSync.exe %{buildroot}/%{_libdir}/dataspace-sync/DataSpaceSync.exe


%files
#%doc
%{_bindir}/dataspacesync
%{_bindir}/dsscli
%{_libdir}/dataspace-sync
%{_datadir}/applications/dataspacesync.desktop
%{_datadir}/dataspace-sync
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-error.png
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-i.png
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-iiii.png
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-iiiii.png
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-ii.png
%{_datadir}/icons/ubuntu-mono-dark/status/24/process-syncing-iii.png
%{_datadir}/icons/hicolor/16x16/apps/app-cmissync.png
%{_datadir}/icons/hicolor/32x32/apps/app-cmissync.png
%{_datadir}/icons/hicolor/24x24/apps/app-cmissync.png
%{_datadir}/icons/hicolor/22x22/apps/app-cmissync.png
%{_datadir}/icons/hicolor/48x48/apps/app-cmissync.png
%{_datadir}/icons/hicolor/256x256/apps/app-cmissync.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-error.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-i.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-iiii.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-iiiii.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-ii.png
%{_datadir}/icons/ubuntu-mono-light/status/24/process-syncing-iii.png


%changelog

