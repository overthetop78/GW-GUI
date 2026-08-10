namespace GWGUI.Scp;

/// <summary>
/// Représente une révolution capturée d'une piste SCP et ses intervalles de flux décodés.
/// </summary>
/// <param name="IndexTimeTicks">Durée de la révolution, exprimée en pas temporels SCP.</param>
/// <param name="DeclaredFluxCount">Nombre de mots de flux déclaré dans le descripteur SCP ; il peut différer du nombre d'intervalles après fusion des marqueurs de dépassement.</param>
/// <param name="FluxIntervals">Intervalles entre transitions magnétiques, exprimés en pas temporels SCP.</param>
public sealed record ScpRevolution(uint IndexTimeTicks, uint DeclaredFluxCount, IReadOnlyList<uint> FluxIntervals)
{
    /// <summary>
    /// Convertit la durée déclarée de la révolution en millisecondes.
    /// </summary>
    /// <param name="resolutionNanoseconds">Durée strictement positive d'un pas temporel, en nanosecondes.</param>
    /// <returns>Durée de la révolution en millisecondes.</returns>
    public double DurationMilliseconds(int resolutionNanoseconds) => IndexTimeTicks * resolutionNanoseconds / 1_000_000d;

    /// <summary>
    /// Calcule la vitesse de rotation correspondant à la durée déclarée de la révolution.
    /// </summary>
    /// <param name="resolutionNanoseconds">Durée strictement positive d'un pas temporel, en nanosecondes.</param>
    /// <returns>Vitesse de rotation en tours par minute.</returns>
    public double Rpm(int resolutionNanoseconds) => 60_000d / DurationMilliseconds(resolutionNanoseconds);
}
