package cn.com.heaton.shiningmask.base.music;

import android.media.MediaPlayer;
import android.media.audiofx.Visualizer;
import cn.com.heaton.shiningmask.sevice.FFTDataListenter;
import cn.com.heaton.shiningmask.sevice.MusicFftListenter;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class VisualizerManager {
    private static final long INTERVAL = 100;
    private static final String TAG = "VisualizerManager";
    private static long lastClickTime;
    private static VisualizerManager sVisualizerManager;
    private List<FFTDataListenter> fftDataListenters = new ArrayList();
    private Visualizer visualizer;

    public static VisualizerManager getInstance() {
        MusicPlayer musicPlayer;
        try {
            if (sVisualizerManager == null && (musicPlayer = ConnectActivity.getMusicPlayer()) != null) {
                sVisualizerManager = new VisualizerManager(musicPlayer.getMediaPlayer());
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return sVisualizerManager;
    }

    private VisualizerManager(MediaPlayer mediaPlayer) {
        if (mediaPlayer != null) {
            startVisualezer(mediaPlayer);
        }
    }

    public void startVisualezer(MediaPlayer mediaPlayer) {
        if (mediaPlayer != null) {
            stop();
            Visualizer visualizer = new Visualizer(mediaPlayer.getAudioSessionId());
            this.visualizer = visualizer;
            visualizer.setCaptureSize(Visualizer.getCaptureSizeRange()[0]);
            VisualizerView visualizerView = new VisualizerView();
            this.visualizer.setDataCaptureListener(visualizerView, Visualizer.getMaxCaptureRate(), true, true);
            this.visualizer.setEnabled(true);
            visualizerView.setMusicFftListenter(new MusicFftListenter() { // from class: cn.com.heaton.shiningmask.base.music.VisualizerManager.1
                @Override // cn.com.heaton.shiningmask.sevice.MusicFftListenter
                public void onStart(byte[] bArr) {
                    LogUtil.d("fftDataListenters:" + VisualizerManager.this.fftDataListenters + "=" + Music.isOpenedRhythm() + " byte:" + ByteUtils.binaryToHexString(bArr));
                    if (VisualizerManager.filter() || !Music.isOpenedRhythm()) {
                        return;
                    }
                    Iterator it = VisualizerManager.this.fftDataListenters.iterator();
                    while (it.hasNext()) {
                        ((FFTDataListenter) it.next()).onStart(bArr);
                    }
                }
            });
        }
    }

    public void stopVisualezer() {
        Visualizer visualizer = this.visualizer;
        if (visualizer != null) {
            visualizer.setEnabled(false);
            this.visualizer.release();
            this.visualizer = null;
        }
    }

    public void setVisualezerEnable(boolean z) {
        LogUtil.d("setVisualezerEnable:" + z + " visualizer:" + this.visualizer);
        if (z) {
            startVisualezer(ConnectActivity.getMusicPlayer().getMediaPlayer());
        } else {
            stopVisualezer();
        }
    }

    public void setFftDataListenter(FFTDataListenter fFTDataListenter) {
        LogUtil.d("setFftDataListenter:" + fFTDataListenter);
        if (this.fftDataListenters.contains(fFTDataListenter)) {
            return;
        }
        this.fftDataListenters.add(fFTDataListenter);
    }

    public void stop() {
        LogUtil.d("stop:" + this.visualizer);
        Visualizer visualizer = this.visualizer;
        if (visualizer == null) {
            return;
        }
        visualizer.setEnabled(false);
        this.visualizer.setDataCaptureListener(null, Visualizer.getMaxCaptureRate(), true, true);
        this.visualizer.release();
        this.visualizer = null;
        VisualizerView.setIsExit(true);
        System.gc();
    }

    public void removeFftDataListenter(FFTDataListenter fFTDataListenter) {
        LogUtil.d("removeFftDataListenter:" + fFTDataListenter);
        if (this.fftDataListenters.contains(fFTDataListenter)) {
            this.fftDataListenters.remove(fFTDataListenter);
        }
    }

    public void clear() {
        LogUtil.d("clear:" + sVisualizerManager);
        sVisualizerManager = null;
    }

    public static boolean filter() {
        long jCurrentTimeMillis = System.currentTimeMillis();
        long j = jCurrentTimeMillis - lastClickTime;
        if (0 < j && j < INTERVAL) {
            return true;
        }
        lastClickTime = jCurrentTimeMillis;
        return false;
    }
}