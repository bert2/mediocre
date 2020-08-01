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

    var step2 = Read("./bin/Release/netcoreapp3.1/avgs-step-2.csv".Rel());
    var step2Errs = step2.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    step2.AvgStdDev(x => x.ms).Dump("step 2 ms");
    step2Errs.AvgStdDev(x => x).Dump("step 2 err");

    var step4 = Read("./bin/Release/netcoreapp3.1/avgs-step-4.csv".Rel());
    var step4Errs = step4.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
    step4.AvgStdDev(x => x.ms).Dump("step 4 ms");
    step4Errs.AvgStdDev(x => x).Dump("step 4 err");

//    var simd = Read("./bin/Release/netcoreapp3.1/avgs-simd.csv".Rel());
//    var simdErrs = simd.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
//    simd.AvgStdDev(x => x.ms).Dump("simd ms");
//    simdErrs.AvgStdDev(x => x).Dump("simd err");
//    
//    var bicubic = Read("./bin/Release/netcoreapp3.1/avgs-bicubic.csv".Rel());
//    var bicubicErrs = bicubic.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
//    bicubic.AvgStdDev(x => x.ms).Dump("bicubic interp ms");
//    bicubicErrs.AvgStdDev(x => x).Dump("bicubic interp err");
//
//    var bilinear = Read("./bin/Release/netcoreapp3.1/avgs-bilinear.csv".Rel());
//    var bilinearErrs = bilinear.Zip(baseline).Select(x => DistVec(x.First, x.Second)).Select(Len).ToArray();
//    bilinear.AvgStdDev(x => x.ms).Dump("bilinear interp ms");
//    bilinearErrs.AvgStdDev(x => x).Dump("bilinear interp err");
}

private (int r, int g, int b) DistVec((byte r, byte g, byte b, long _) x, (byte r, byte g, byte b, long _) baseline)
    => (x.r - baseline.r, x.g - baseline.g, x.b - baseline.b);
    
private double Len((int r, int g, int b) x) => Math.Sqrt(x.r * x.r + x.g * x.g + x.b * x.b);

private static (byte r, byte g, byte b, long ms)[] Read(string file) => new StreamReader(file)
    .AsCsv(separator: ',')
    .Select(r => (r: byte.Parse(r[0]), g: byte.Parse(r[1]), b: byte.Parse(r[2]), ms: long.Parse(r[3])))
    .ToArray();
