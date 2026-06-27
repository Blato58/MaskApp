package cn.com.heaton.shiningmask.base.app;

import android.content.Context;
import android.media.MediaPlayer;

/* JADX INFO: loaded from: classes.dex */
public class BackgroundPlayer {
    public static MediaPlayer start(Context context, int i) {
        MediaPlayer mediaPlayerCreate = MediaPlayer.create(context, i);
        mediaPlayerCreate.start();
        mediaPlayerCreate.pause();
        mediaPlayerCreate.setLooping(true);
        return mediaPlayerCreate;
    }
}