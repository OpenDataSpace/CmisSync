SUBDIRS = ubuntu-mono-dark \
	ubuntu-mono-light

theme = hicolor
system_themedir = $(datadir)/icons/$(theme)
app_themedir = $(pkgdatadir)/icons/$(theme)

system_theme_icons = \
	apps,dataspacesync-app-16.png \
	apps,dataspacesync-app-22.png \
	apps,dataspacesync-app-24.png \
	apps,dataspacesync-app-32.png \
	apps,dataspacesync-app-48.png \
	apps,dataspacesync-app-256.png

app_theme_icons = \
	places,dataspacesync-folder-16.png \
	places,dataspacesync-folder-22.png \
	places,dataspacesync-folder-24.png \
	places,dataspacesync-folder-32.png \
	places,dataspacesync-folder-48.png \
	places,dataspacesync-folder-256.png \
	status,dataspacesync-process-syncing-i-24.png \
	status,dataspacesync-process-syncing-ii-24.png \
	status,dataspacesync-process-syncing-iii-24.png \
	status,dataspacesync-process-syncing-iiii-24.png \
	status,dataspacesync-process-syncing-iiiii-24.png \
	status,dataspacesync-process-syncing-error-24.png \
	status,dataspacesync-start-16.png \
	status,dataspacesync-pause-16.png \
	status,dataspacesync-downloading-16.png \
	status,dataspacesync-uploading-16.png \
	status,dataspacesync-updating-16.png \
	status,dataspacesync-deleted-12.png

install_icon_exec = $(top_srcdir)/build/icon-theme-installer \
	-t "$(theme)" \
	-s "$(srcdir)" \
	-d "x$(DESTDIR)" \
	-m "$(mkinstalldirs)" \
	-x "$(INSTALL_DATA)"

install-data-local:
	@-$(install_icon_exec) -i -b "$(system_themedir)" $(system_theme_icons)
	@-$(install_icon_exec) -i -b "$(app_themedir)" $(app_theme_icons)

uninstall-hook:
	@-$(install_icon_exec) -u -b "$(system_themedir)" $(system_theme_icons)
	@-$(install_icon_exec) -u -b "$(app_themedir)" $(app_theme_icons)

MAINTAINERCLEANFILES = Makefile.in
EXTRA_DIST = $(wildcard *.png *.svg)
