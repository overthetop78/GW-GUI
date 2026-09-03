using System.Runtime.ExceptionServices;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Dictionaries;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests;

public sealed class EmulationVideoSettingsSectionTests
{
    [Fact]
    public void ModuleSettingsCanRebuildWithItsReusableVideoPanel()
    {
        RunSta(() =>
        {
            var application = Application.Current ?? new Application();
            application.Resources[typeof(ComboBox)] = new Style(typeof(ComboBox));
            application.Resources[typeof(ComboBoxItem)] = new Style(typeof(ComboBoxItem));
            application.Resources["MainTabItemStyle"] = new Style(typeof(TabItem));
            application.Resources["IconFontFamily"] = new System.Windows.Media.FontFamily(
                "Segoe MDL2 Assets");
            var section = new EmulationModuleSettingsSection(new TestModule());
            var rebuild = typeof(EmulationModuleSettingsSection).GetMethod(
                "RebuildEditor", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(rebuild);
            rebuild.Invoke(section, null);
            Assert.NotNull(section.Content);
        });
    }

    [Fact]
    public void TechnologyPanelsAreConditionalAndGeneralAdjustmentsStayVisible()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            var technologyMarkers = new Dictionary<EmulationVideoDisplayTechnology, string>
            {
                [EmulationVideoDisplayTechnology.Crt] = EmulationVideoProcessingCatalog.CrtBeamWidth,
                [EmulationVideoDisplayTechnology.FixedPixel] =
                    EmulationVideoProcessingCatalog.FixedPixelGridIntensity,
                [EmulationVideoDisplayTechnology.Plasma] =
                    EmulationVideoProcessingCatalog.PlasmaCellStructure,
                [EmulationVideoDisplayTechnology.Vector] =
                    EmulationVideoProcessingCatalog.VectorLineThreshold,
                [EmulationVideoDisplayTechnology.Vfd] =
                    EmulationVideoProcessingCatalog.VfdPhosphorIntensity,
                [EmulationVideoDisplayTechnology.LedMatrix] =
                    EmulationVideoProcessingCatalog.LedMatrixCellSize,
                [EmulationVideoDisplayTechnology.DotMatrix] =
                    EmulationVideoProcessingCatalog.DotMatrixDotSize,
                [EmulationVideoDisplayTechnology.SegmentDisplay] =
                    EmulationVideoProcessingCatalog.SegmentDisplayThickness,
                [EmulationVideoDisplayTechnology.EPaper] =
                    EmulationVideoProcessingCatalog.EPaperContrast,
                [EmulationVideoDisplayTechnology.Projection] =
                    EmulationVideoProcessingCatalog.ProjectionOpticalBlur
            };
            var permanent = new[]
            {
                EmulationVideoProcessingCatalog.Brightness,
                EmulationVideoProcessingCatalog.Contrast,
                EmulationVideoProcessingCatalog.Gamma,
                EmulationVideoProcessingCatalog.Saturation,
                EmulationVideoProcessingCatalog.Sharpness,
                EmulationVideoProcessingCatalog.Dedithering,
                EmulationVideoProcessingCatalog.Denoising,
                EmulationVideoProcessingCatalog.Debanding,
                EmulationVideoProcessingCatalog.DetailRecovery,
                EmulationVideoProcessingCatalog.Deinterlacing,
                EmulationVideoProcessingCatalog.GeneralPersistence,
                EmulationVideoProcessingCatalog.MotionBlur,
                EmulationVideoProcessingCatalog.Flicker,
                EmulationVideoProcessingCatalog.Interlacing,
                EmulationVideoProcessingCatalog.BlackFrameInsertion,
                EmulationVideoProcessingCatalog.SignalConnection,
                EmulationVideoProcessingCatalog.Grain,
                EmulationVideoProcessingCatalog.Vhs,
                EmulationVideoProcessingCatalog.ChromaticAberration,
                EmulationVideoProcessingCatalog.Bloom,
                EmulationVideoProcessingCatalog.Sepia
            };

            foreach (var technology in Enum.GetValues<EmulationVideoDisplayTechnology>())
            {
                panel.SetConfiguration(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = technology
                });
                var ids = AutomationIds(panel);
                Assert.All(permanent, id => Assert.Contains(id, ids));
                foreach (var marker in technologyMarkers)
                    Assert.Equal(marker.Key == technology, ids.Contains(marker.Value));
            }
        });
    }

    [Fact]
    public void ImageParametersUseFiveVerticalSlidersAndPreserveCombinedChanges()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            var ids = new[]
            {
                EmulationVideoProcessingCatalog.Brightness,
                EmulationVideoProcessingCatalog.Contrast,
                EmulationVideoProcessingCatalog.Gamma,
                EmulationVideoProcessingCatalog.Saturation,
                EmulationVideoProcessingCatalog.Sharpness
            };
            var sliders = ids.Select(id => FindByAutomationId<Slider>(panel, id)).ToArray();

            Assert.All(sliders, slider =>
            {
                Assert.Equal(Orientation.Vertical, slider.Orientation);
                Assert.Equal(220, slider.Height);
            });
            Assert.Single(sliders.Select(slider =>
                Assert.IsAssignableFrom<FrameworkElement>(slider.Parent).Parent).Distinct());

            sliders[0].Value = 3;
            sliders[1].Value = -2;
            Assert.Equal(3, panel.Configuration.Adjustments.Brightness);
            Assert.Equal(-2, panel.Configuration.Adjustments.Contrast);
        });
    }
    [Fact]
    public void RestorationSettingsUseCompactControlsAndPreserveCombinedChanges()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                Restoration = new EmulationImageRestorationConfiguration(
                    Dedithering: 33, Denoising: 20, Debanding: 30,
                    DetailRecovery: 40, Deinterlacing: EmulationDeinterlacingMode.Blend)
            });

            var continuous = new[]
            {
                EmulationVideoProcessingCatalog.Denoising,
                EmulationVideoProcessingCatalog.Debanding,
                EmulationVideoProcessingCatalog.DetailRecovery
            }.Select(id => FindByAutomationId<Slider>(panel, id)).ToArray();
            Assert.All(continuous, slider =>
            {
                Assert.Equal(Orientation.Vertical, slider.Orientation);
                Assert.Equal(150, slider.Height);
            });

            var dedithering = FindByAutomationId<Slider>(panel,
                EmulationVideoProcessingCatalog.Dedithering);
            Assert.Equal(Orientation.Horizontal, dedithering.Orientation);
            Assert.Equal(0, dedithering.Minimum);
            Assert.Equal(3, dedithering.Maximum);
            Assert.Equal(1, dedithering.TickFrequency);
            Assert.Equal(TickPlacement.BottomRight, dedithering.TickPlacement);
            var levelTicks = FindByAutomationId<TickBar>(panel,
                "Video.Dedithering.LevelTicks");
            Assert.Equal(0, levelTicks.Minimum);
            Assert.Equal(3, levelTicks.Maximum);
            Assert.Equal(1, levelTicks.TickFrequency);

            var deditheringBlock = FindByAutomationId<StackPanel>(panel,
                "Video.Dedithering.Block");
            deditheringBlock.Measure(new Size(220, double.PositiveInfinity));
            deditheringBlock.Arrange(new Rect(0, 0, 220,
                deditheringBlock.DesiredSize.Height));
            deditheringBlock.UpdateLayout();
            Assert.InRange(deditheringBlock.ActualWidth, 1, 220);
            Assert.InRange(levelTicks.ActualWidth, 1, deditheringBlock.ActualWidth);
            for (var index = 0; index < 4; index++)
            {
                var level = FindByAutomationId<TextBlock>(deditheringBlock,
                    $"Video.Dedithering.Level.{index}");
                var bounds = level.TransformToAncestor(deditheringBlock)
                    .TransformBounds(new Rect(level.RenderSize));
                Assert.True(bounds.Left >= 0);
                Assert.True(bounds.Right <= deditheringBlock.ActualWidth);
            }

            dedithering.Value = 2;

            continuous[0].Value = 55;
            Assert.Equal(67, panel.Configuration.Restoration.Dedithering);
            Assert.Equal(55, panel.Configuration.Restoration.Denoising);

            var deinterlacing = FindByAutomationId<ComboBox>(panel,
                EmulationVideoProcessingCatalog.Deinterlacing);
            Assert.Equal(220, deinterlacing.Width);
        });
    }
    [Fact]
    public void SignalPanelUsesOneConnectionAndOneStandardWithSharedIntensities()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                SignalSimulation = new(EmulationSignalConnection.SVideo, 35,
                    EmulationSignalStandard.Pal, 45)
            });

            var connection = FindByAutomationId<ComboBox>(panel,
                EmulationVideoProcessingCatalog.SignalConnection);
            var connectionIntensity = FindByAutomationId<Slider>(panel,
                EmulationVideoProcessingCatalog.SignalConnectionIntensity);
            var standard = FindByAutomationId<ComboBox>(panel,
                EmulationVideoProcessingCatalog.SignalStandard);
            var standardIntensity = FindByAutomationId<Slider>(panel,
                EmulationVideoProcessingCatalog.SignalStandardIntensity);

            Assert.Equal(6, connection.Items.Count);
            Assert.Equal(4, standard.Items.Count);
            Assert.Equal(35, connectionIntensity.Value);
            Assert.Equal(45, standardIntensity.Value);
            connection.SelectedIndex = (int)EmulationSignalConnection.Rf;
            Assert.Equal(EmulationSignalConnection.Rf,
                panel.Configuration.SignalSimulation.Connection);
            standard = FindByAutomationId<ComboBox>(panel,
                EmulationVideoProcessingCatalog.SignalStandard);
            standard.SelectedIndex = (int)EmulationSignalStandard.Secam;
            Assert.Equal(EmulationSignalStandard.Secam,
                panel.Configuration.SignalSimulation.Standard);
        });
    }

    [Fact]
    public void SignalPanelHidesDependentControlsWhenConnectionIsNone()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                SignalSimulation = new(EmulationSignalConnection.None, 80,
                    EmulationSignalStandard.Secam, 70)
            });

            var identifiers = AutomationIds(panel);
            Assert.Contains(EmulationVideoProcessingCatalog.SignalConnection, identifiers);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.SignalConnectionIntensity,
                identifiers);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.SignalStandard, identifiers);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.SignalStandardIntensity,
                identifiers);
        });
    }

    [Fact]
    public void TemporalEffectsUseThreeHorizontalIntensitiesAndSeparateInterlacingControls()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                Temporal = new EmulationTemporalVideoConfiguration(
                    GeneralPersistence: 10, MotionBlur: 20, Flicker: 30,
                    Interlacing: 0, InterlacingVisibility: 40)
            });

            var continuous = new[]
            {
                EmulationVideoProcessingCatalog.GeneralPersistence,
                EmulationVideoProcessingCatalog.MotionBlur,
                EmulationVideoProcessingCatalog.Flicker
            }.Select(id => FindByAutomationId<Slider>(panel, id)).ToArray();
            Assert.All(continuous, slider =>
            {
                Assert.Equal(Orientation.Horizontal, slider.Orientation);
                Assert.Equal(0, slider.MinWidth);
            });

            var interlacing = FindByAutomationId<CheckBox>(panel,
                EmulationVideoProcessingCatalog.Interlacing);
            var visibility = FindByAutomationId<Slider>(panel,
                EmulationVideoProcessingCatalog.InterlacingVisibility);
            var blackFrames = FindByAutomationId<CheckBox>(panel,
                EmulationVideoProcessingCatalog.BlackFrameInsertion);
            Assert.False(visibility.IsEnabled);

            interlacing.IsChecked = true;
            Assert.True(visibility.IsEnabled);
            visibility.Value = 65;
            blackFrames.IsChecked = true;
            Assert.Equal(100, panel.Configuration.Temporal.Interlacing);
            Assert.Equal(65, panel.Configuration.Temporal.InterlacingVisibility);
            Assert.True(panel.Configuration.Temporal.BlackFrameInsertion);

            interlacing.IsChecked = false;
            Assert.Equal(0, panel.Configuration.Temporal.Interlacing);
            Assert.Equal(65, panel.Configuration.Temporal.InterlacingVisibility);
        });
    }
    [Fact]
    public void EveryVideoGroupIsPlacedInTabsWithCompactCards()
    {
        RunSta(() =>
        {
            var renderer = new ComboBox();
            var rendererChoice = EmulationSettingsLayout.VideoSettingsChoice(
                new EmulationVideoSettingsField("Rendu", renderer));
            var panel = new EmulationVideoProcessingSettingsSection();
            var display = new Grid();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt
            }, display, rendererChoice);

            var tabs = Assert.IsType<TabControl>(panel.Content);
            Assert.Equal(new[] { "Display", "Technology", "Image", "Effects" },
                tabs.Items.Cast<TabItem>().Select(item => item.Tag));
            var tabBodies = tabs.Items.Cast<TabItem>()
                .Select(item =>
                {
                    var scroller = Assert.IsType<ScrollViewer>(item.Content);
                    Assert.Equal(ScrollBarVisibility.Auto,
                        scroller.VerticalScrollBarVisibility);
                    Assert.Equal(ScrollBarVisibility.Disabled,
                        scroller.HorizontalScrollBarVisibility);
                    return Assert.IsType<Border>(scroller.Content);
                }).ToArray();
            Assert.All(tabBodies, body =>
            {
                Assert.True(body.MaxWidth is 760 or 1140);
                Assert.Equal(HorizontalAlignment.Stretch, body.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Top, body.VerticalAlignment);
            });
            Assert.Contains(display, Descendants(tabBodies[0]));
            Assert.Contains(renderer, Descendants(tabBodies[0]));
            Assert.Contains(EmulationVideoProcessingCatalog.CrtBeamWidth,
                AutomationIds(tabBodies[1]));
            Assert.Equal("CRT", tabs.Items.Cast<TabItem>()
                .Single(item => Equals(item.Tag, "Technology")).Header);
        });
    }

    [Fact]
    public void ConditionalRebuildPreservesTheSelectedVideoTab()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt
            });
            var tabs = Assert.IsType<TabControl>(panel.Content);
            tabs.SelectedItem = tabs.Items.Cast<TabItem>().Single(item =>
                Equals(item.Tag, "Technology"));

            var scanlines = FindByAutomationId<CheckBox>(panel,
                EmulationVideoProcessingCatalog.CrtScanlinesEnabled);
            scanlines.IsChecked = true;

            var rebuiltTabs = Assert.IsType<TabControl>(panel.Content);
            Assert.Equal("Technology",
                Assert.IsType<TabItem>(rebuiltTabs.SelectedItem).Tag);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlineIntensity,
                AutomationIds(panel));
        });
    }

    [Fact]
    public void TechnologyTabIsHiddenForNormalDisplay()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Normal
            }, new Grid(), new ComboBox());

            var tabs = Assert.IsType<TabControl>(panel.Content);
            Assert.DoesNotContain(tabs.Items.Cast<TabItem>(), item =>
                Equals(item.Tag, "Technology"));
            Assert.Equal("Display", Assert.IsType<TabItem>(tabs.SelectedItem).Tag);
        });
    }

    [Fact]
    public void ScanlineParametersAppearOnlyForEnabledCrtScanlines()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt
            });
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlinesEnabled,
                AutomationIds(panel));
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtScanlineIntensity,
                AutomationIds(panel));

            panel.SetConfiguration(panel.Configuration with
            {
                Crt = panel.Configuration.Crt with { ScanlinesEnabled = true }
            });
            var enabledIds = AutomationIds(panel);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlineOrientation, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlineIntensity, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlineThickness, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlinePhase, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtScanlineCompensation, enabledIds);

            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Normal,
                Crt = new EmulationCrtVideoConfiguration(ScanlinesEnabled: true)
            });
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtScanlinesEnabled,
                AutomationIds(panel));
        });
    }

    [Fact]
    public void VoluntaryPatternParametersAppearOnlyForEnabledCrtPattern()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt
            });
            Assert.Contains(EmulationVideoProcessingCatalog.CrtPatternEnabled,
                AutomationIds(panel));
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtPatternIntensity,
                AutomationIds(panel));

            panel.SetConfiguration(panel.Configuration with
            {
                Crt = panel.Configuration.Crt with { PatternEnabled = true }
            });
            var enabledIds = AutomationIds(panel);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtPatternOrientation, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtPatternFrequency, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtPatternPhase, enabledIds);
            Assert.Contains(EmulationVideoProcessingCatalog.CrtPatternIntensity, enabledIds);

            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                Crt = new EmulationCrtVideoConfiguration(PatternEnabled: true)
            });
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtPatternEnabled,
                AutomationIds(panel));
        });
    }

    [Fact]
    public void FixedPixelTechnologiesShareOnePanelAndOnlyBacklightIsConditional()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            Assert.Equal(Enum.GetValues<EmulationFixedPixelTechnology>(),
                EmulationVideoProcessingCatalog.FixedPixelTechnologyResourceKeys.Keys);

            foreach (var technology in Enum.GetValues<EmulationFixedPixelTechnology>())
            {
                panel.SetConfiguration(new EmulationVideoProcessingConfiguration
                {
                    DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
                    FixedPixel = new EmulationFixedPixelVideoConfiguration(Technology: technology,
                        BacklightIntensity: 50, BlackDepth: 50)
                });
                var ids = AutomationIds(panel);
                Assert.Single(ids, id =>
                    id == EmulationVideoProcessingCatalog.FixedPixelTechnology);
                Assert.Contains(EmulationVideoProcessingCatalog.FixedPixelGridIntensity, ids);
                Assert.Contains(EmulationVideoProcessingCatalog.FixedPixelBlackDepth, ids);
                Assert.Equal(technology != EmulationFixedPixelTechnology.Oled,
                    ids.Contains(EmulationVideoProcessingCatalog.FixedPixelBacklight));
                Assert.Equal(technology != EmulationFixedPixelTechnology.Oled,
                    ids.Contains(EmulationVideoProcessingCatalog.FixedPixelBacklightBleed));
                Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtBeamWidth, ids);
                Assert.DoesNotContain(EmulationVideoProcessingCatalog.PlasmaCellStructure, ids);
                Assert.DoesNotContain(EmulationVideoProcessingCatalog.VectorLineThreshold, ids);
            }
        });
    }

    [Fact]
    public void PlasmaPanelContainsOnlyItsFourDocumentedParameters()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            var plasma = new EmulationPlasmaVideoConfiguration(
                CellStructure: 21, Diffusion: 32,
                TemporalDithering: 43, PersistenceIntensity: 54);
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Plasma,
                Plasma = plasma
            });
            var ids = AutomationIds(panel);

            Assert.Contains(EmulationVideoProcessingCatalog.PlasmaCellStructure, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.PlasmaDiffusion, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.PlasmaTemporalDithering, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.PlasmaPersistence, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtBeamWidth, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.FixedPixelGridIntensity, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.VectorLineThreshold, ids);
            Assert.Equal(plasma, panel.Configuration.Plasma);
        });
    }

    [Fact]
    public void VectorPanelContainsOnlyItsRasterApproximationParameters()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            var vector = new EmulationVectorVideoConfiguration(
                LineThreshold: 25, LineIntensity: 50,
                HaloIntensity: 40, PersistenceIntensity: 30);
            panel.SetConfiguration(new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Vector,
                Vector = vector
            });
            var ids = AutomationIds(panel);

            Assert.Contains(EmulationVideoProcessingCatalog.VectorLineThreshold, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.VectorLineIntensity, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.VectorHaloIntensity, ids);
            Assert.Contains(EmulationVideoProcessingCatalog.VectorPersistence, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.CrtBeamWidth, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.FixedPixelGridIntensity, ids);
            Assert.DoesNotContain(EmulationVideoProcessingCatalog.PlasmaCellStructure, ids);
            Assert.Equal(vector, panel.Configuration.Vector);
        });
    }

    [Fact]
    public void ChangingDisplayTechnologyAppliesImmediatelyWithoutConfirmation()
    {
        RunSta(() =>
        {
            var panel = new EmulationVideoProcessingSettingsSection();
            var original = new EmulationVideoProcessingConfiguration
            {
                DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
                Sampling = EmulationVideoSampling.Sabr,
                Restoration = new EmulationImageRestorationConfiguration(Dedithering: 35,
                    Denoising: 20, Debanding: 15, DetailRecovery: 25,
                    Deinterlacing: EmulationDeinterlacingMode.Blend)
            };
            panel.SetConfiguration(original);

            SelectTechnology(panel, EmulationVideoDisplayTechnology.FixedPixel);
            Assert.Equal(EmulationVideoDisplayTechnology.FixedPixel,
                panel.Configuration.DisplayTechnology);
            Assert.Equal(original.Sampling, panel.Configuration.Sampling);
            Assert.Equal(original.Restoration, panel.Configuration.Restoration);

            SelectTechnology(panel, EmulationVideoDisplayTechnology.Vector);
            Assert.Equal(EmulationVideoDisplayTechnology.Vector,
                panel.Configuration.DisplayTechnology);
        });
    }

    [Fact]
    public async Task UnsavedConfigurationChangesAreKeptAsDraft()
    {
        var module = new TestModule();
        var configuration = (TestConfiguration)module.CreateConfiguration(TestModule.MachineId)
            with
        {
            VideoProcessing = new EmulationVideoProcessingConfiguration
            {
                Adjustments = new EmulationImageAdjustments(Brightness: 4)
            }
        };
        EmulationConfigurationDraftStore.Remove(module.Id, TestModule.MachineId);
        try
        {
            var saved = await EmulationConfigurationPersistenceFunctions.PersistAsync(
                module, configuration, hasSavedConfiguration: false);

            Assert.False(saved);
            Assert.True(EmulationConfigurationDraftStore.TryGet(
                module.Id, TestModule.MachineId, out var draft));
            Assert.Equal(4, draft.VideoProcessing.Adjustments.Brightness);
            Assert.Empty(module.SavedConfigurations);
        }
        finally
        {
            EmulationConfigurationDraftStore.Remove(module.Id, TestModule.MachineId);
        }
    }

    [Fact]
    public async Task ExistingConfigurationChangesAreSavedAutomatically()
    {
        var existing = TestConfiguration.Create() with
        {
            VideoProcessing = new EmulationVideoProcessingConfiguration
            {
                Adjustments = new EmulationImageAdjustments(Brightness: 6)
            }
        };
        var module = new TestModule(existing);

        var wasSaved = await EmulationConfigurationPersistenceFunctions.PersistAsync(
            module, existing, hasSavedConfiguration: true);

        Assert.True(wasSaved);
        var saved = Assert.Single(module.SavedConfigurations);
        Assert.Same(existing, saved);
        Assert.Equal(existing.Id, saved.Id);
        Assert.Equal(6, saved.VideoProcessing.Adjustments.Brightness);
    }

    [Fact]
    public void ConfigurationChangeTargetsOnlyTheMatchingOpenInstance()
    {
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var open = new Dictionary<(string ModuleId, Guid ConfigurationId), string>
        {
            [("amiga", targetId)] = "target",
            [("amiga", otherId)] = "other configuration",
            [("atari", targetId)] = "other module"
        };
        var applied = new List<string>();

        var found = EmulationOpenMachineConfigurationFunctions.TryApply(
            open, "amiga", targetId, applied.Add);
        var missing = EmulationOpenMachineConfigurationFunctions.TryApply(
            open, "missing", targetId, applied.Add);

        Assert.True(found);
        Assert.False(missing);
        Assert.Equal(["target"], applied);
    }

    private static void SelectTechnology(EmulationVideoProcessingSettingsSection panel,
        EmulationVideoDisplayTechnology technology)
    {
        var selector = FindByAutomationId<ComboBox>(
            panel, nameof(EmulationVideoDisplayTechnology));
        selector.SelectedItem = selector.Items.Cast<object>().Single(item =>
            Equals(item.GetType().GetProperty("Value")!.GetValue(item), technology));
    }

    private static IReadOnlySet<string> AutomationIds(DependencyObject root) =>
        Descendants(root).Select(AutomationProperties.GetAutomationId)
            .Where(id => !string.IsNullOrEmpty(id)).ToHashSet(StringComparer.Ordinal);

    private static T FindByAutomationId<T>(DependencyObject root, string id)
        where T : DependencyObject =>
        Descendants(root).OfType<T>().Single(element =>
            AutomationProperties.GetAutomationId(element) == id);

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var descendant in Descendants(child))
                yield return descendant;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { failure = error; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    private sealed record TestConfiguration(
        string ModuleId,
        Guid Id,
        string MachineId,
        EmulationVideoRenderer VideoRenderer,
        EmulationVideoProcessingConfiguration VideoProcessing) : IEmulationConfiguration
    {
        internal static TestConfiguration Create() => new(
            $"test-{Guid.NewGuid():N}", Guid.NewGuid(), TestModule.MachineId,
            EmulationVideoRenderer.Wpf, new EmulationVideoProcessingConfiguration());
    }

    private sealed class TestModule : IEmulationModule
    {
        internal const string MachineId = "test-machine";
        private readonly IReadOnlyList<IEmulationConfiguration> _loaded;

        internal TestModule(params IEmulationConfiguration[] loaded)
        {
            _loaded = loaded;
            Id = loaded.FirstOrDefault()?.ModuleId ?? $"test-{Guid.NewGuid():N}";
        }

        public string Id { get; }
        public string DisplayResourceKey => "Emulation.Video.Technology.Normal";
        public IReadOnlyList<EmulationMachineDefinition> Machines { get; } =
            [new(MachineId, "Emulation.Video.Technology.Normal")];
        public EmulationSettingsVisibility DefaultVisibility => Visibility;
        internal List<IEmulationConfiguration> SavedConfigurations { get; } = [];

        public bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
        {
            exitCode = 0;
            return false;
        }

        public EmulationMachineSettings Describe(string machineId,
            IEmulationConfiguration? configuration = null) =>
            new(machineId, Visibility, []);

        public IEmulationConfiguration CreateConfiguration(string machineId) =>
            new TestConfiguration(Id, Guid.NewGuid(), machineId, EmulationVideoRenderer.Wpf,
                new EmulationVideoProcessingConfiguration());

        public IEmulationConfiguration ChangeMachine(
            IEmulationConfiguration configuration, string machineId) =>
            ((TestConfiguration)configuration) with { MachineId = machineId };

        public IEmulationConfiguration ApplySettings(IEmulationConfiguration configuration,
            IReadOnlyDictionary<string, string?> values) => configuration;

        public IEmulationConfiguration ApplyVideoProcessing(IEmulationConfiguration configuration,
            EmulationVideoProcessingConfiguration videoProcessing) =>
            ((TestConfiguration)configuration) with { VideoProcessing = videoProcessing };

        public EmulationConfigurationSummary SummarizeConfiguration(
            IEmulationConfiguration configuration) =>
            new("Emulation.Video.Technology.Normal", []);

        public ValueTask<EmulationMachineRuntime> CreateRuntimeAsync(
            IEmulationConfiguration configuration, EmulationRuntimeServices services,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public ValueTask<IReadOnlyList<IEmulationConfiguration>> LoadConfigurationsAsync(
            CancellationToken cancellationToken = default) => ValueTask.FromResult(_loaded);

        public ValueTask SaveConfigurationAsync(IEmulationConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            SavedConfigurations.Add(configuration);
            return ValueTask.CompletedTask;
        }

        public ValueTask DeleteConfigurationAsync(Guid configurationId,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        private static EmulationSettingsVisibility Visibility { get; } = new(
            new Dictionary<EmulationMachineTab, bool>
            {
                [EmulationMachineTab.General] = true,
                [EmulationMachineTab.Video] = true
            });
    }
}
