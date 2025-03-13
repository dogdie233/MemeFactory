﻿using System.Runtime.CompilerServices;
using MemeFactory.Core.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MemeFactory.Core.Utilities;

public static class Transformers
{
    public static async IAsyncEnumerable<Frame> Rotation(this IAsyncEnumerable<Frame> frames, int circleTimes = 16,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)

    {
        float deg;
        var total = 0;
        var allFrames = await frames.ToListAsync(cancellationToken);
        if (allFrames.Count > circleTimes)
        {
            deg = (allFrames.Count * 1f / circleTimes) * 360f / allFrames.Count;
        }
        else
        {
            (total, circleTimes) = Enumerable.Range(circleTimes - 2, 4)
                .Select(c => (Algorithms.Lcm(allFrames.Count, c) / allFrames.Count, c))
                .MinBy(p => p.Item1);
            deg = 360f / circleTimes;
        }

        var baseSize = allFrames[0].Image.Size;
        foreach (var frame in allFrames.Loop(total).ToList()) using (frame)
        {
            frame.Image.Mutate((ctx) => ctx.Rotate(deg * frame.Sequence - 1));
            var newFrame = new Image<Rgba32>(baseSize.Width, baseSize.Height);
            var x = (baseSize.Width - frame.Image.Width) / 2;
            var y = (baseSize.Height - frame.Image.Height) / 2;
            newFrame.Mutate((ctx) => ctx.DrawImage(frame.Image, new Point(x, y), 1f));
            yield return frame with { Image = newFrame };
        }
    }
}