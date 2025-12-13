using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class RevekSongSuppressor : MonoBehaviour
{
    private void OnEnable() => RevekSongSummon.AddInterceptor(InterceptRevekSong);

    private void OnDisable() => RevekSongSummon.RemoveInterceptor(InterceptRevekSong);

    protected virtual bool InterceptRevekSong(List<FluteNote> song) => true;
}
