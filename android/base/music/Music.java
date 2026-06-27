package cn.com.heaton.shiningmask.base.music;

import java.util.List;
import java.util.Random;

/* JADX INFO: loaded from: classes.dex */
public class Music {
    private static int STATE = 0;
    public static final int STATE_PAUSED = 2;
    public static final int STATE_PLAYBACK_COMPLETED = 0;
    public static final int STATE_STARTED = 1;
    private static final String TAG = "Music";
    private static Song currentSong;
    private static boolean isOpenedRhythm;
    private static int orderState;
    private static int playMode;
    private static List<Song> songList;

    public static int getSTATE() {
        return STATE;
    }

    public static void setSTATE(int i) {
        STATE = i;
    }

    public static List<Song> getSongList() {
        return songList;
    }

    public static void setSongList(List<Song> list) {
        songList = list;
    }

    public static Song getCurrentSong() {
        return currentSong;
    }

    public static void setCurrentSong(Song song) {
        currentSong = song;
    }

    public static int getPlayMode() {
        return playMode;
    }

    public static void setPlayMode(int i) {
        playMode = i;
    }

    public static boolean isOpenedRhythm() {
        return isOpenedRhythm;
    }

    public static void setIsOpenedRhythm(boolean z) {
        isOpenedRhythm = z;
    }

    public static int getOrderState() {
        return orderState;
    }

    public static void setOrderState(int i) {
        orderState = i;
    }

    public static Song getNextSong() {
        int iIndexOf = songList.indexOf(currentSong) + 1;
        if (songList.size() <= iIndexOf) {
            iIndexOf = 0;
        }
        Song song = getSong(iIndexOf);
        currentSong = song;
        return song;
    }

    public static Song getLastSong() {
        int iIndexOf = songList.indexOf(currentSong) - 1;
        if (iIndexOf < 0) {
            iIndexOf = songList.size() - 1;
        }
        Song song = getSong(iIndexOf);
        currentSong = song;
        return song;
    }

    public static Song getRandomSong() {
        Song song = getSong(new Random().nextInt(songList.size()));
        currentSong = song;
        return song;
    }

    public static Song getSong(int i) {
        List<Song> list = songList;
        if (list != null && list.size() > 0 && i >= 0 && songList.size() > i) {
            return songList.get(i);
        }
        return null;
    }
}