package cn.com.heaton.shiningmask.base.recoder;

import android.content.Context;
import android.media.AudioManager;
import android.media.MediaPlayer;
import android.media.MediaRecorder;
import android.os.Bundle;
import android.os.Environment;
import java.io.File;
import java.io.IOException;

/* JADX INFO: loaded from: classes.dex */
public class MediaRecordHelper implements MediaPlayer.OnCompletionListener, MediaPlayer.OnErrorListener {
    public static final int IDLE_STATE = 0;
    public static final int INTERNAL_ERROR = 2;
    public static final int IN_CALL_RECORD_ERROR = 3;
    public static final int NO_ERROR = 0;
    public static final int PLAYING_STATE = 2;
    public static final int RECORDING_STATE = 1;
    static final String SAMPLE_LENGTH_KEY = "sample_length";
    static final String SAMPLE_PATH_KEY = "sample_path";
    static final String SAMPLE_PREFIX = "recording";
    public static final int SDCARD_ACCESS_ERROR = 1;
    int mState = 0;
    OnStateChangedListener mOnStateChangedListener = null;
    long mSampleStart = 0;
    int mSampleLength = 0;
    File mSampleFile = null;
    MediaRecorder mRecorder = null;
    MediaPlayer mPlayer = null;

    public interface OnStateChangedListener {
        void onError(int i);

        void onStateChanged(int i);
    }

    public void saveState(Bundle bundle) {
        bundle.putString(SAMPLE_PATH_KEY, this.mSampleFile.getAbsolutePath());
        bundle.putInt(SAMPLE_LENGTH_KEY, this.mSampleLength);
    }

    public int getMaxAmplitude() {
        if (this.mState != 1) {
            return 0;
        }
        return this.mRecorder.getMaxAmplitude();
    }

    public void restoreState(Bundle bundle) {
        int i;
        String string = bundle.getString(SAMPLE_PATH_KEY);
        if (string == null || (i = bundle.getInt(SAMPLE_LENGTH_KEY, -1)) == -1) {
            return;
        }
        File file = new File(string);
        if (file.exists()) {
            File file2 = this.mSampleFile;
            if (file2 == null || file2.getAbsolutePath().compareTo(file.getAbsolutePath()) != 0) {
                delete();
                this.mSampleFile = file;
                this.mSampleLength = i;
                signalStateChanged(0);
            }
        }
    }

    public void setOnStateChangedListener(OnStateChangedListener onStateChangedListener) {
        this.mOnStateChangedListener = onStateChangedListener;
    }

    public int state() {
        return this.mState;
    }

    public int progress() {
        int i = this.mState;
        if (i == 1 || i == 2) {
            return (int) ((System.currentTimeMillis() - this.mSampleStart) / 1000);
        }
        return 0;
    }

    public int sampleLength() {
        return this.mSampleLength;
    }

    public File sampleFile() {
        return this.mSampleFile;
    }

    public void delete() {
        stop();
        File file = this.mSampleFile;
        if (file != null) {
            file.delete();
        }
        this.mSampleFile = null;
        this.mSampleLength = 0;
        signalStateChanged(0);
    }

    public void clear() {
        stop();
        this.mSampleLength = 0;
        signalStateChanged(0);
    }

    public void startRecording(int i, String str, Context context) {
        stop();
        if (this.mSampleFile == null) {
            File externalStorageDirectory = Environment.getExternalStorageDirectory();
            if (!externalStorageDirectory.canWrite()) {
                externalStorageDirectory = new File("/sdcard/sdcard");
            }
            try {
                this.mSampleFile = File.createTempFile(SAMPLE_PREFIX, str, externalStorageDirectory);
            } catch (IOException unused) {
                setError(1);
                return;
            }
        }
        MediaRecorder mediaRecorder = new MediaRecorder();
        this.mRecorder = mediaRecorder;
        mediaRecorder.setAudioSource(1);
        this.mRecorder.setOutputFormat(i);
        this.mRecorder.setAudioEncoder(1);
        this.mRecorder.setOutputFile(this.mSampleFile.getAbsolutePath());
        try {
            this.mRecorder.prepare();
            try {
                this.mRecorder.start();
                this.mSampleStart = System.currentTimeMillis();
                setState(1);
            } catch (RuntimeException unused2) {
                AudioManager audioManager = (AudioManager) context.getSystemService("audio");
                if (audioManager.getMode() == 2 || audioManager.getMode() == 3) {
                    setError(3);
                } else {
                    setError(2);
                }
                this.mRecorder.reset();
                this.mRecorder.release();
                this.mRecorder = null;
            }
        } catch (IOException unused3) {
            setError(2);
            this.mRecorder.reset();
            this.mRecorder.release();
            this.mRecorder = null;
        }
    }

    public void stopRecording() {
        MediaRecorder mediaRecorder = this.mRecorder;
        if (mediaRecorder == null) {
            return;
        }
        mediaRecorder.stop();
        this.mRecorder.release();
        this.mRecorder = null;
        this.mSampleLength = (int) ((System.currentTimeMillis() - this.mSampleStart) / 1000);
        setState(0);
    }

    public void startPlayback() {
        stop();
        MediaPlayer mediaPlayer = new MediaPlayer();
        this.mPlayer = mediaPlayer;
        try {
            mediaPlayer.setDataSource(this.mSampleFile.getAbsolutePath());
            this.mPlayer.setOnCompletionListener(this);
            this.mPlayer.setOnErrorListener(this);
            this.mPlayer.prepare();
            this.mPlayer.start();
            this.mSampleStart = System.currentTimeMillis();
            setState(2);
        } catch (IOException unused) {
            setError(1);
            this.mPlayer = null;
        } catch (IllegalArgumentException unused2) {
            setError(2);
            this.mPlayer = null;
        }
    }

    public void stopPlayback() {
        MediaPlayer mediaPlayer = this.mPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.stop();
        this.mPlayer.release();
        this.mPlayer = null;
        setState(0);
    }

    public void stop() {
        stopRecording();
        stopPlayback();
    }

    @Override // android.media.MediaPlayer.OnErrorListener
    public boolean onError(MediaPlayer mediaPlayer, int i, int i2) {
        stop();
        setError(1);
        return true;
    }

    @Override // android.media.MediaPlayer.OnCompletionListener
    public void onCompletion(MediaPlayer mediaPlayer) {
        stop();
    }

    private void setState(int i) {
        if (i == this.mState) {
            return;
        }
        this.mState = i;
        signalStateChanged(i);
    }

    private void signalStateChanged(int i) {
        OnStateChangedListener onStateChangedListener = this.mOnStateChangedListener;
        if (onStateChangedListener != null) {
            onStateChangedListener.onStateChanged(i);
        }
    }

    private void setError(int i) {
        OnStateChangedListener onStateChangedListener = this.mOnStateChangedListener;
        if (onStateChangedListener != null) {
            onStateChangedListener.onError(i);
        }
    }
}