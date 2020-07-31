<Query Kind="Program">
  <NuGetReference>Nullable.Extensions</NuGetReference>
  <Namespace>MoreLinq</Namespace>
  <Namespace>Nullable.Extensions</Namespace>
  <Namespace>Nullable.Extensions.Util</Namespace>
  <Namespace>static Nullable.Extensions.NullableClass</Namespace>
  <Namespace>static Nullable.Extensions.NullableStruct</Namespace>
  <Namespace>static Nullable.Extensions.Util.TryParseFunctions</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main() {
    var baseline = Read("./bin/Release/netcoreapp3.1/avgs-baseline.csv".Rel());
    baseline.AvgStdDev(x => x.ms).Dump("baseline ms");

    //var tpl = Read("./bin/Release/netcoreapp3.1/avgs-tpl.csv".Rel());
    //var tplErrs = tpl.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    //tpl.AvgStdDev(x => x.ms).Dump("tpl ms");
    //tplErrs.AvgStdDev(x => x).Dump("tpl err");

    var simd = Read("./bin/Release/netcoreapp3.1/avgs-simd.csv".Rel());
    var simdErrs = simd.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    simd.AvgStdDev(x => x.ms).Dump("simd ms");
    simdErrs.AvgStdDev(x => x).Dump("simd err");

    var bicubicHq = Read("./bin/Release/netcoreapp3.1/avgs-bicubic-hq.csv".Rel());
    var bicubicHqErrs = bicubicHq.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    bicubicHq.AvgStdDev(x => x.ms).Dump("bicubic hq ms");
    bicubicHqErrs.AvgStdDev(x => x).Dump("bicubic hq err");
    
    var bicubic1x = Read("./bin/Release/netcoreapp3.1/avgs-bicubic-1x.csv".Rel());
    var bicubic1xErrs = bicubic1x.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    bicubic1x.AvgStdDev(x => x.ms).Dump("bicubic 1x ms");
    bicubic1xErrs.AvgStdDev(x => x).Dump("bicubic 1x err");

    var bicubic2x = Read("./bin/Release/netcoreapp3.1/avgs-bicubic-2x.csv".Rel());
    var bicubic2xErrs = bicubic2x.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    bicubic2x.AvgStdDev(x => x.ms).Dump("bicubic 2x ms");
    bicubic2xErrs.AvgStdDev(x => x).Dump("bicubic 2x err");

    var bicubic3x = Read("./bin/Release/netcoreapp3.1/avgs-bicubic-3x.csv".Rel());
    var bicubic3xErrs = bicubic3x.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    bicubic3x.AvgStdDev(x => x.ms).Dump("bicubic 3x ms");
    bicubic3xErrs.AvgStdDev(x => x).Dump("bicubic 3x err");

    var bicubic4x = Read("./bin/Release/netcoreapp3.1/avgs-bicubic-4x.csv".Rel());
    var bicubic4xErrs = bicubic4x.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    bicubic4x.AvgStdDev(x => x.ms).Dump("bicubic 4x ms");
    bicubic4xErrs.AvgStdDev(x => x).Dump("bicubic 4x err");
}

private (int r, int g, int b) DistVec((byte r, byte g, byte b, long _) x, (byte r, byte g, byte b, long _) baseline)
    => (x.r - baseline.r, x.g - baseline.g, x.b - baseline.b);
    
private double Len((int r, int g, int b) x) => Math.Sqrt(x.r * x.r + x.g * x.g + x.b * x.b);

private static (byte r, byte g, byte b, long ms)[] Read(string file) => new StreamReader(file)
    .AsCsv(separator: ',')
    .Select(r => (r: byte.Parse(r[0]), g: byte.Parse(r[1]), b: byte.Parse(r[2]), ms: long.Parse(r[3])))
    .ToArray();
