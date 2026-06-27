package cn.com.heaton.shiningmask.base.recoder;

import android.media.AudioRecord;
import android.media.audiofx.Visualizer;
import android.os.Looper;
import android.util.Log;

/* JADX INFO: loaded from: classes.dex */
public class SoundRecordHelper {
    AudioRecord audioRecord;
    private int audioSource;
    SoundRecoderHelperCallbackData callbackData;
    private Visualizer mVisualizer;
    private BaseVisualizerView mVisualizerView;
    public int minRecBufSize;
    private int recAudioFormat;
    private int recChannel;
    private int recSampleRate;
    RecordThread recThread;
    private boolean recordFlag;

    public interface SoundRecoderHelperCallbackData {
        void reportdata(short[] sArr);
    }

    public boolean isRecording() {
        return this.recordFlag;
    }

    public int getAudioSessionid() {
        return this.audioRecord.getAudioSessionId();
    }

    public SoundRecordHelper() {
        this.recSampleRate = 16000;
        this.recChannel = 2;
        this.recAudioFormat = 2;
        this.audioSource = 1;
        this.recordFlag = false;
        this.audioRecord = null;
        this.recThread = null;
        this.callbackData = null;
        this.minRecBufSize = AudioRecord.getMinBufferSize(16000, 2, 2);
        this.audioRecord = new AudioRecord(this.audioSource, this.recSampleRate, this.recChannel, this.recAudioFormat, this.minRecBufSize);
    }

    public SoundRecordHelper(int i, int i2, int i3) {
        this.recSampleRate = 16000;
        this.recChannel = 2;
        this.recAudioFormat = 2;
        this.audioSource = 1;
        this.recordFlag = false;
        this.audioRecord = null;
        this.recThread = null;
        this.callbackData = null;
        this.minRecBufSize = AudioRecord.getMinBufferSize(16000, 2, 2);
        this.recSampleRate = i;
        this.recChannel = i2;
        this.recAudioFormat = i3;
        this.minRecBufSize = AudioRecord.getMinBufferSize(i, i2, i3);
        this.audioRecord = new AudioRecord(this.audioSource, this.recSampleRate, this.recChannel, this.recAudioFormat, this.minRecBufSize);
    }

    public void start() {
        this.audioRecord.startRecording();
        RecordThread recordThread = new RecordThread(this.audioRecord, this.minRecBufSize);
        this.recThread = recordThread;
        this.recordFlag = true;
        recordThread.start();
    }

    public void stop() {
        this.recordFlag = false;
        this.recThread = null;
    }

    public void setDatareportCallBack(SoundRecoderHelperCallbackData soundRecoderHelperCallbackData) {
        this.callbackData = soundRecoderHelperCallbackData;
    }

    public class RecordThread extends Thread {
        private AudioRecord ar;
        private int bufSize;

        public RecordThread(AudioRecord audioRecord, int i) {
            this.ar = audioRecord;
            this.bufSize = i;
        }

        @Override // java.lang.Thread, java.lang.Runnable
        public void run() {
            Looper.prepare();
            try {
                short[] sArr = new short[this.bufSize];
                while (SoundRecordHelper.this.recordFlag) {
                    if (this.ar.read(sArr, 0, this.bufSize) == -2) {
                        SoundRecordHelper.this.recordFlag = false;
                    } else {
                        SoundRecordHelper.this.callbackData.reportdata(sArr);
                    }
                }
                this.ar.stop();
            } catch (Exception e) {
                Log.e("Receive message E", e.toString());
            }
        }
    }
}