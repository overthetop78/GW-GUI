using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Reconstruction.Scp;

/// <summary>Vue non destructive d'une piste SCP préparée pour le décodage.</summary>
/// <param name="Flux">Flux original d'une révolution ou flux continu de plusieurs révolutions successives.</param>
/// <param name="Revolution">Révolution source à base un, ou zéro lorsque la vue agrège plusieurs révolutions.</param>
/// <param name="IsContinuous">Indique que la vue raccorde plusieurs révolutions successives.</param>
internal sealed record ScpTrackDecodeWindow(FluxRevolution Flux, int Revolution, bool IsContinuous);
