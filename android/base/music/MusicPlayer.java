package cn.com.heaton.shiningmask.base.music;

import android.app.Service;
import android.content.Intent;
import android.database.Cursor;
import android.media.AudioManager;
import android.media.AudioRecord;
import android.media.MediaMetadataRetriever;
import android.media.MediaPlayer;
import android.os.Binder;
import android.os.IBinder;
import android.provider.MediaStore;
import android.text.TextUtils;
import androidx.constraintlayout.core.motion.utils.TypedValues;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class MusicPlayer extends Service implements MediaPlayer.OnPreparedListener, MediaPlayer.OnErrorListener, MediaPlayer.OnCompletionListener, MediaPlayer.OnSeekCompleteListener {
    private MediaPlayer mediaPlayer;
    private List<MusicListenter> musicListenterList = new ArrayList();

    @Override // android.app.Service
    public void onCreate() {
        super.onCreate();
        if (this.mediaPlayer == null) {
            MediaPlayer mediaPlayer = new MediaPlayer();
            this.mediaPlayer = mediaPlayer;
            mediaPlayer.setAudioStreamType(3);
            this.mediaPlayer.setOnErrorListener(this);
            this.mediaPlayer.setOnPreparedListener(this);
            this.mediaPlayer.setOnCompletionListener(this);
            this.mediaPlayer.setOnSeekCompleteListener(this);
        }
    }

    @Override // android.app.Service
    public IBinder onBind(Intent intent) {
        return new BinderImpl();
    }

    @Override // android.app.Service
    public boolean onUnbind(Intent intent) {
        return super.onUnbind(intent);
    }

    @Override // android.app.Service
    public void onDestroy() {
        super.onDestroy();
    }

    public List<Song> searchAndOrderSongName() {
        return searchAndSortOrder("_display_name");
    }

    public List<Song> searchAndOrderSinger() {
        return searchAndSortOrder("artist");
    }

    public List<Song> searchAndSortOrder(String str) {
        ArrayList arrayList = new ArrayList();
        Cursor cursorQuery = getContentResolver().query(MediaStore.Audio.Media.EXTERNAL_CONTENT_URI, new String[]{"_id", "_display_name", "title", TypedValues.TransitionType.S_DURATION, "artist", "album", "year", "mime_type", "_size", "_data"}, "mime_type=? or mime_type=?", new String[]{"audio/mpeg", "audio/x-ms-wma"}, "title");
        if (cursorQuery != null && cursorQuery.moveToFirst()) {
            do {
                Song song = new Song();
                song.setFileName(cursorQuery.getString(1));
                song.setTitle(cursorQuery.getString(2));
                song.setDuration(cursorQuery.getInt(3));
                String string = cursorQuery.getString(4);
                LogUtil.d("歌曲名：" + cursorQuery.getString(2) + "  歌手名:" + string);
                if (TextUtils.isEmpty(string) || string.equals("<unknown>") || "未知歌手".equals(string)) {
                    String string2 = getResources().getString(R.string.unkown_artist);
                    LogUtil.d("歌曲名1：" + cursorQuery.getString(2) + "  歌手名:" + string2);
                    song.setSinger(string2);
                } else {
                    song.setSinger(cursorQuery.getString(4));
                    LogUtil.d("歌曲名2：" + cursorQuery.getString(2) + "  歌手名:" + string);
                }
                song.setAlbum(cursorQuery.getString(5));
                if (cursorQuery.getString(6) != null) {
                    song.setYear(cursorQuery.getString(6));
                } else {
                    song.setYear("Unknown");
                }
                if ("audio/mpeg".equals(cursorQuery.getString(7).trim())) {
                    song.setType("mp3");
                } else if ("audio/x-ms-wma".equals(cursorQuery.getString(7).trim())) {
                    song.setType("wma");
                }
                if (cursorQuery.getString(8) == null) {
                    song.setSize("Unknown");
                }
                if (cursorQuery.getString(9) != null) {
                    song.setFileUrl(cursorQuery.getString(9));
                }
                if ((song.getType().equals("mp3") || song.getType().equals("ogg")) && song.getDuration() > 3000) {
                    arrayList.add(song);
                }
            } while (cursorQuery.moveToNext());
            cursorQuery.close();
        }
        return arrayList;
    }

    public static String getRingDuring(String str) {
        MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
        if (str != null) {
            try {
                try {
                    HashMap map = new HashMap();
                    map.put("User-Agent", "Mozilla/5.0 (Linux; U; Android 4.4.2; zh-CN; MW-KW-001 Build/JRO03C) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 UCBrowser/1.0.0.001 U4/0.8.0 Mobile Safari/533.1");
                    mediaMetadataRetriever.setDataSource(str, map);
                } catch (Exception e) {
                    e.printStackTrace();
                    try {
                        mediaMetadataRetriever.release();
                        return null;
                    } catch (IOException e2) {
                        throw new RuntimeException(e2);
                    }
                }
            } catch (Throwable th) {
                try {
                    mediaMetadataRetriever.release();
                    throw th;
                } catch (IOException e3) {
                    throw new RuntimeException(e3);
                }
            }
        }
        String strExtractMetadata = mediaMetadataRetriever.extractMetadata(9);
        try {
            mediaMetadataRetriever.release();
            return strExtractMetadata;
        } catch (IOException e4) {
            throw new RuntimeException(e4);
        }
    }

    public List<Song> searchBySongName(String str) {
        int i;
        ArrayList arrayList = new ArrayList();
        int i2 = 0;
        Cursor cursorQuery = getContentResolver().query(MediaStore.Audio.Media.EXTERNAL_CONTENT_URI, new String[]{"_id", "_display_name", "title", TypedValues.TransitionType.S_DURATION, "artist", "album", "year", "mime_type", "_size", "_data"}, "title like? ", new String[]{"%" + str + "%"}, null);
        if (cursorQuery == null || !cursorQuery.moveToFirst()) {
            return arrayList;
        }
        while (true) {
            Song song = new Song();
            song.setFileName(cursorQuery.getString(1));
            song.setTitle(cursorQuery.getString(2));
            song.setDuration(cursorQuery.getInt(3));
            song.setSinger(cursorQuery.getString(4));
            song.setAlbum(cursorQuery.getString(5));
            if (cursorQuery.getString(6) != null) {
                song.setYear(cursorQuery.getString(6));
            } else {
                song.setYear("Unknown");
            }
            if ("audio/mpeg".equals(cursorQuery.getString(7).trim())) {
                song.setType("mp3");
            } else if ("audio/x-ms-wma".equals(cursorQuery.getString(7).trim())) {
                song.setType("wma");
            }
            if (cursorQuery.getString(8) != null) {
                i = i2;
                song.setSize((((cursorQuery.getInt(8) / 1024.0f) / 1024.0f) + "").substring(i, 4) + "M");
            } else {
                i = i2;
                song.setSize("Unknown");
            }
            if (cursorQuery.getString(9) != null) {
                song.setFileUrl(cursorQuery.getString(9));
            }
            arrayList.add(song);
            if (!cursorQuery.moveToNext()) {
                cursorQuery.close();
                return arrayList;
            }
            i2 = i;
        }
    }

    public void play(String str) throws IOException {
        if (str == null || str.isEmpty()) {
            return;
        }
        new AudioRecord(1, 16000, 2, 2, AudioRecord.getMinBufferSize(16000, 2, 2));
        reset();
        this.mediaPlayer.setDataSource(str);
        this.mediaPlayer.prepare();
        this.mediaPlayer.start();
    }

    public void rePlay(String str) throws IOException {
        if (str == null || str.isEmpty()) {
            return;
        }
        play(str);
    }

    public void start() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.start();
        for (int i = 0; i < this.musicListenterList.size(); i++) {
            MusicListenter musicListenter = this.musicListenterList.get(i);
            if (musicListenter != null) {
                musicListenter.onStart();
            }
        }
    }

    public void pause() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.pause();
        for (int i = 0; i < this.musicListenterList.size(); i++) {
            MusicListenter musicListenter = this.musicListenterList.get(i);
            if (musicListenter != null) {
                musicListenter.onPause();
            }
        }
    }

    public void stop() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.stop();
    }

    public void reset() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.reset();
    }

    public int getVolume() {
        return ((AudioManager) getSystemService("audio")).getStreamVolume(3);
    }

    public void setVolume(int i) {
        AudioManager audioManager = (AudioManager) getSystemService("audio");
        int streamMaxVolume = audioManager.getStreamMaxVolume(3);
        if (i < 0) {
            i = 0;
        }
        if (streamMaxVolume >= i) {
            streamMaxVolume = i;
        }
        audioManager.setStreamVolume(3, streamMaxVolume, 0);
    }

    public int getDuration() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return -1;
        }
        return mediaPlayer.getDuration();
    }

    public int getCurrentPosition() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return -1;
        }
        return mediaPlayer.getCurrentPosition();
    }

    public void seekTo(int i) {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.seekTo(i);
    }

    public boolean isLooping() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return false;
        }
        return mediaPlayer.isLooping();
    }

    public boolean isPlaying() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return false;
        }
        return mediaPlayer.isPlaying();
    }

    public void release() {
        MediaPlayer mediaPlayer = this.mediaPlayer;
        if (mediaPlayer == null) {
            return;
        }
        mediaPlayer.release();
        this.mediaPlayer = null;
    }

    @Override // android.media.MediaPlayer.OnErrorListener
    public boolean onError(MediaPlayer mediaPlayer, int i, int i2) {
        for (int i3 = 0; i3 < this.musicListenterList.size(); i3++) {
            this.musicListenterList.get(i3).onError(mediaPlayer, i, i2);
        }
        return false;
    }

    @Override // android.media.MediaPlayer.OnPreparedListener
    public void onPrepared(MediaPlayer mediaPlayer) {
        if (mediaPlayer != null) {
            mediaPlayer.start();
        }
        for (int i = 0; i < this.musicListenterList.size(); i++) {
            MusicListenter musicListenter = this.musicListenterList.get(i);
            if (musicListenter != null) {
                musicListenter.onStart();
            }
        }
    }

    @Override // android.media.MediaPlayer.OnCompletionListener
    public void onCompletion(MediaPlayer mediaPlayer) {
        for (int i = 0; i < this.musicListenterList.size(); i++) {
            LogUtil.d("===播放完成");
            this.musicListenterList.get(i).onCompletion(mediaPlayer);
        }
    }

    @Override // android.media.MediaPlayer.OnSeekCompleteListener
    public void onSeekComplete(MediaPlayer mediaPlayer) {
        for (int i = 0; i < this.musicListenterList.size(); i++) {
            this.musicListenterList.get(i).onSeekComplete(mediaPlayer);
        }
    }

    public class BinderImpl extends Binder {
        public BinderImpl() {
        }

        public MusicPlayer getService() {
            return MusicPlayer.this;
        }
    }

    public void registerMusicListenter(MusicListenter musicListenter) {
        this.musicListenterList.add(musicListenter);
    }

    public void unregisterMusicListenter(MusicListenter musicListenter) {
        this.musicListenterList.remove(musicListenter);
    }

    public MediaPlayer getMediaPlayer() {
        return this.mediaPlayer;
    }
}