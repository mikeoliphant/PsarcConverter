using System;
using UILayout;
using PsarcConverter;

using var host = new PsarcConverterHost(800, 600, isFullscreen: false);

host.UseEmbeddedResources = true;

MonoGameLayout layout = new MonoGameLayout();

host.StartGame(layout);