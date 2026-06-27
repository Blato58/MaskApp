package cn.com.heaton.shiningmask.ui.activity.test;

import android.graphics.drawable.AnimationDrawable;
import android.os.CountDownTimer;
import android.os.Handler;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.animation.DecelerateInterpolator;
import android.view.animation.TranslateAnimation;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import android.widget.SeekBar;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.app.SoundManager;
import cn.com.heaton.shiningmask.base.music.Music;
import cn.com.heaton.shiningmask.base.music.MusicListenter;
import cn.com.heaton.shiningmask.base.music.MusicPlayer;
import cn.com.heaton.shiningmask.base.music.Song;
import cn.com.heaton.shiningmask.base.music.VisualizerManager;
import cn.com.heaton.shiningmask.databinding.ActivityRhythm3Binding;
import cn.com.heaton.shiningmask.sevice.FFTDataListenter;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.adapter.SongAdapter;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.DensityUtil;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView;
import com.blankj.utilcode.constant.CacheConstants;
import java.io.IOException;
import java.util.List;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class RhythmActivity extends BaseActivity<ActivityRhythm3Binding> implements AdapterView.OnItemClickListener, SeekBar.OnSeekBarChangeListener, View.OnClickListener {
    private static final String TAG = "RhythmActivity";
    private AnimationDrawable animBtn;
    private DataManager dataManager;
    private FFTDataListenter fftDataListenter;
    private boolean isPlayVisible;
    private AnimationDrawable ivForwardAnim;
    private String minuteStr;
    MusicListenterImpl musicListenter;
    MusicPlayer musicPlayer;
    private String secondStr;
    SongAdapter songAdapter;
    List<Song> songList;
    private boolean rhythmStatus = true;
    private int curSelectMode = 0;
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.6
        @Override // java.lang.Runnable
        public void run() {
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).rlRhyShow.setVisibility(0);
        }
    };
    CountDownTimer playPositionTimer = new CountDownTimer(86400000, 1000) { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.7
        @Override // android.os.CountDownTimer
        public void onFinish() {
        }

        @Override // android.os.CountDownTimer
        public void onTick(long j) {
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).sbPlayTime.setProgress(RhythmActivity.this.musicPlayer.getCurrentPosition());
        }
    };
    Handler handler = new Handler();

    @Override // android.widget.SeekBar.OnSeekBarChangeListener
    public void onStartTrackingTouch(SeekBar seekBar) {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityRhythm3Binding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityRhythm3Binding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().top.ivBack.setOnClickListener(this);
        getBinding().imgvPlayMode.setOnClickListener(this);
        getBinding().imgvLast.setOnClickListener(this);
        getBinding().imgvPlayPause.setOnClickListener(this);
        getBinding().imgvNext.setOnClickListener(this);
        getBinding().ivPlayList.setOnClickListener(this);
        getBinding().top.ivForward.setOnClickListener(this);
        getBinding().top.ivBack.setImageResource(R.mipmap.text_magic_back);
        getBinding().top.ivBack.setVisibility(0);
        getBinding().top.ivForward.setImageResource(R.mipmap.rhy_microphone);
        getBinding().top.ivForward.setVisibility(0);
        getBinding().sbPlayTime.setOnSeekBarChangeListener(this);
        getBinding().ivAnim.setImageResource(R.drawable.anim_music_rhy);
        AnimationDrawable animationDrawable = (AnimationDrawable) getBinding().ivAnim.getDrawable();
        this.animBtn = animationDrawable;
        animationDrawable.start();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.musicPlayer = ConnectActivity.getMusicPlayer();
        this.dataManager = DataManager.getInstance();
        this.curSelectMode = DataManager.getInstance().getCurSelectRhythmMode();
        this.songList = Music.getSongList();
        this.songAdapter = new SongAdapter(this, this.songList);
        getBinding().lstvSong.setAdapter((ListAdapter) this.songAdapter);
        getBinding().lstvSong.setOnItemClickListener(this);
        initMusicPlayer();
        getBinding().rhyledview1.setLayerType(1, null);
        getBinding().rhyledview1.setMode(0);
        getBinding().rhyledview1.setPointMargin(0);
        getBinding().rhyledview1.removeAllViews();
        getBinding().rhyledview1.init(36, 12);
        getBinding().imageSwitchView.setCurrentImage(DataManager.getInstance().getCurSelectRhythmMode1());
    }

    private void initSongList() {
        try {
            this.songList = this.musicPlayer.searchAndSortOrder(null);
            LogUtil.d("歌曲：" + this.songList.size());
            Music.setSongList(this.songList);
            List<Song> list = this.songList;
            if (list == null || list.isEmpty()) {
                return;
            }
            Music.setCurrentSong(this.songList.get(0));
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().imageSwitchView.setOnImageItemClickListener(new RhyImage3DSwitchView.OnImageItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.1
            @Override // cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView.OnImageItemClickListener
            public void onClick(int i) {
            }

            @Override // cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView.OnImageItemClickListener
            public void onTouch() {
                RhythmActivity.this.rhyUIShow(false);
            }

            @Override // cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView.OnImageItemClickListener
            public void onTouchStop() {
                RhythmActivity.this.rhyUIShow(true);
            }
        });
        getBinding().imageSwitchView.setOnImageSwitchListener(new RhyImage3DSwitchView.OnImageSwitchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.2
            @Override // cn.com.heaton.shiningmask.ui.widget.image3d.RhyImage3DSwitchView.OnImageSwitchListener
            public void onImageSwitch(int i) {
                LogUtil.d("currentImage:" + i);
                RhythmActivity rhythmActivity = RhythmActivity.this;
                rhythmActivity.rhyUIShow((rhythmActivity.musicPlayer != null && RhythmActivity.this.musicPlayer.isPlaying()) || DataManager.getInstance().isMicrophoneEnable());
                RhythmActivity.this.dataManager.setCurSelectRhythmMode1(i);
                switch (i) {
                    case 0:
                    case 5:
                        RhythmActivity.this.selectRhythmMode(1);
                        break;
                    case 1:
                    case 6:
                        RhythmActivity.this.selectRhythmMode(2);
                        break;
                    case 2:
                    case 7:
                        RhythmActivity.this.selectRhythmMode(3);
                        break;
                    case 3:
                    case 8:
                        RhythmActivity.this.selectRhythmMode(4);
                        break;
                    case 4:
                    case 9:
                        RhythmActivity.this.selectRhythmMode(5);
                        break;
                }
            }
        });
        this.fftDataListenter = new FFTDataListenter() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.3
            @Override // cn.com.heaton.shiningmask.sevice.FFTDataListenter
            public void onStart(byte[] bArr) {
                if (!DataManager.getInstance().isMicrophoneEnable() && RhythmActivity.this.rhythmStatus && Music.isOpenedRhythm() && RhythmActivity.this.musicPlayer != null && RhythmActivity.this.musicPlayer.isPlaying()) {
                    RhythmActivity.this.setRhyData(bArr);
                }
            }
        };
        if (VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().setFftDataListenter(this.fftDataListenter);
        }
        getBinding().rlBottom.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.4
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                int action = motionEvent.getAction();
                if (action == 0) {
                    motionEvent.getY();
                } else if (action == 1) {
                    float y = motionEvent.getY();
                    if (y - 0.0f <= -5.0f) {
                        LogUtil.d("y1:0.0  y2:" + y);
                        RhythmActivity.this.songListVisible();
                    }
                } else if (action == 2) {
                    motionEvent.getY();
                }
                return true;
            }
        });
        getBinding().rlSongName.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.5
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                int action = motionEvent.getAction();
                if (action == 0) {
                    motionEvent.getY();
                } else if (action == 1) {
                    float y = motionEvent.getY();
                    if (y - 0.0f >= 5.0f) {
                        LogUtil.d("y1:0.0  y2:" + y);
                        RhythmActivity.this.songListGone();
                    }
                } else if (action == 2) {
                    motionEvent.getY();
                }
                return true;
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setRhyData(byte[] bArr) {
        int i = this.curSelectMode;
        if (i == 0) {
            getBinding().rhyledview1.setRhyData1(bArr);
            return;
        }
        if (i == 1) {
            getBinding().rhyledview1.setRhyData2(bArr);
            return;
        }
        if (i == 2) {
            getBinding().rhyledview1.setRhyData3(bArr);
            return;
        }
        if (i == 3) {
            getBinding().rhyledview1.setRhyData4(bArr);
        } else if (i == 4) {
            getBinding().rhyledview1.setRhyData5(bArr);
        } else {
            getBinding().rhyledview1.setRhyData1(bArr);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        EventBus.getDefault().unregister(this);
        this.dataManager.setCurSelectRhythmMode(this.curSelectMode);
        CountDownTimer countDownTimer = this.playPositionTimer;
        if (countDownTimer != null) {
            countDownTimer.cancel();
        }
        if (VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().removeFftDataListenter(this.fftDataListenter);
        }
    }

    @Override // android.widget.AdapterView.OnItemClickListener
    public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
        Song song;
        if (ClickFilter.filter() || (song = Music.getSong(i)) == null) {
            return;
        }
        try {
            setMicroPhoneOff();
            Music.setCurrentSong(song);
            this.musicPlayer.play(song.getFileUrl());
            getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
            openRhythm();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private void initMusicPlayer() {
        if (this.musicPlayer == null) {
            return;
        }
        MusicListenterImpl musicListenterImpl = new MusicListenterImpl();
        this.musicListenter = musicListenterImpl;
        this.musicPlayer.registerMusicListenter(musicListenterImpl);
        for (int i = 0; i < this.songList.size(); i++) {
            this.songList.get(i).setState(0);
        }
        int playMode = Music.getPlayMode();
        if (playMode == 1) {
            getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_random);
        } else if (playMode == 2) {
            getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_single);
        } else {
            getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_circulation);
        }
        Song currentSong = Music.getCurrentSong();
        if (currentSong != null) {
            getBinding().txvSongName.setText(currentSong.getTitle());
            getBinding().txvSinger.setText(currentSong.getSinger());
            getBinding().txvSongName1.setText(currentSong.getTitle());
            getBinding().txvSinger1.setText(currentSong.getSinger());
            int i2 = 0;
            while (true) {
                if (i2 >= this.songList.size()) {
                    break;
                }
                Song song = this.songList.get(i2);
                song.setState(0);
                if (song != null && song.getFileUrl().equals(currentSong.getFileUrl())) {
                    song.setState(1);
                    break;
                }
                i2++;
            }
        }
        if (Music.getSTATE() == 1) {
            getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
            rhyUIShow(true);
        } else if (Music.getSTATE() == 2) {
            getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_player);
            rhyUIShow(false);
        } else {
            getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_player);
            rhyUIShow(false);
        }
        if (Music.getSTATE() == 1) {
            LogUtil.d("=====duration：" + this.musicPlayer.getDuration());
            getBinding().sbPlayTime.setMax(this.musicPlayer.getDuration());
            this.playPositionTimer.start();
        }
    }

    @Override // android.widget.SeekBar.OnSeekBarChangeListener
    public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
        int i2 = i / 1000;
        long j = ((long) i2) - (((long) (i2 / CacheConstants.HOUR)) * 3600);
        long j2 = j / 60;
        long j3 = j - (60 * j2);
        if (j2 < 10) {
            this.minuteStr = "0" + j2;
        } else {
            this.minuteStr = "" + j2;
        }
        if (j3 < 10) {
            this.secondStr = "0" + j3;
        } else {
            this.secondStr = "" + j3;
        }
        getBinding().txvPlayTime.setText(this.minuteStr + ":" + this.secondStr);
    }

    @Override // android.widget.SeekBar.OnSeekBarChangeListener
    public void onStopTrackingTouch(SeekBar seekBar) {
        this.musicPlayer.seekTo(seekBar.getProgress());
    }

    class MusicListenterImpl extends MusicListenter {
        MusicListenterImpl() {
        }

        @Override // cn.com.heaton.shiningmask.base.music.MusicListenter
        public void onStart() {
            String str;
            String str2;
            super.onStart();
            for (int i = 0; i < RhythmActivity.this.songList.size(); i++) {
                RhythmActivity.this.songList.get(i).setState(0);
            }
            Song currentSong = Music.getCurrentSong();
            if (currentSong != null) {
                for (int i2 = 0; i2 < RhythmActivity.this.songList.size(); i2++) {
                    Song song = RhythmActivity.this.songList.get(i2);
                    if (song != null && currentSong.getFileUrl().equals(song.getFileUrl())) {
                        song.setState(1);
                    }
                }
            }
            RhythmActivity.this.songAdapter.notifyDataSetChanged();
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).txvSongName.setText(currentSong.getTitle());
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).txvSinger.setText(currentSong.getSinger());
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).txvSongName1.setText(currentSong.getTitle());
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).txvSinger1.setText(currentSong.getSinger());
            int duration = RhythmActivity.this.musicPlayer.getDuration();
            int i3 = duration / 1000;
            long j = ((long) i3) - (((long) (i3 / CacheConstants.HOUR)) * 3600);
            long j2 = j / 60;
            long j3 = j - (60 * j2);
            if (j2 < 10) {
                str = "0" + j2;
            } else {
                str = "" + j2;
            }
            if (j3 < 10) {
                str2 = "0" + j3;
            } else {
                str2 = "" + j3;
            }
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).txvPlayDuration.setText(str + ":" + str2);
            ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).sbPlayTime.setMax(duration);
            RhythmActivity.this.playPositionTimer.start();
        }

        @Override // cn.com.heaton.shiningmask.base.music.MusicListenter
        public void onPause() {
            super.onPause();
            RhythmActivity.this.playPositionTimer.cancel();
        }
    }

    private void playOrPause() {
        LogUtil.d("播放或暂停:" + Music.isOpenedRhythm() + "  " + Music.getSTATE());
        try {
            if (Music.getSTATE() == 0) {
                Music.setIsOpenedRhythm(true);
                setMicroPhoneOff();
                Song currentSong = Music.getCurrentSong();
                if (currentSong != null) {
                    this.musicPlayer.play(currentSong.getFileUrl());
                    getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
                    rhyUIShow(true);
                    if (this.rhythmStatus) {
                        openRhythm();
                        return;
                    }
                    return;
                }
                return;
            }
            if (Music.getSTATE() == 1) {
                stopPlayMusic();
            } else if (Music.getSTATE() == 2) {
                startPalyMusic();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private void stopPlayMusic() {
        Music.setIsOpenedRhythm(false);
        if (this.musicPlayer.isPlaying()) {
            this.musicPlayer.pause();
        }
        getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_player);
        Music.setIsOpenedRhythm(false);
        rhyUIShow(false);
    }

    private void startPalyMusic() {
        Music.setIsOpenedRhythm(true);
        setMicroPhoneOff();
        this.musicPlayer.start();
        getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
        openRhythm();
        rhyUIShow(true);
    }

    private void changeSong(Song song) {
        if (song == null) {
            return;
        }
        String fileUrl = song.getFileUrl();
        try {
            if (Music.getSTATE() == 0 || Music.getSTATE() == 1 || Music.getSTATE() == 2) {
                this.musicPlayer.play(fileUrl);
                Music.setSTATE(1);
            }
            if (this.rhythmStatus) {
                openRhythm();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void openRhythm() {
        LogUtil.d("开启律动");
        Music.setIsOpenedRhythm(true);
        this.rhythmStatus = true;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void selectRhythmMode(int i) {
        if (i == 1) {
            this.curSelectMode = 0;
            this.dataManager.setCurSelectRhythmMode(0);
            return;
        }
        if (i == 2) {
            this.curSelectMode = 1;
            this.dataManager.setCurSelectRhythmMode(1);
            return;
        }
        if (i == 3) {
            this.curSelectMode = 2;
            this.dataManager.setCurSelectRhythmMode(2);
        } else if (i == 4) {
            this.curSelectMode = 3;
            this.dataManager.setCurSelectRhythmMode(3);
        } else {
            if (i != 5) {
                return;
            }
            this.curSelectMode = 4;
            this.dataManager.setCurSelectRhythmMode(4);
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        int i = 0;
        if (id == R.id.iv_back) {
            SoundManager.getInstance().textBack();
            DataManager.getInstance().setMicrophoneEnable(false);
            finish();
            return;
        }
        if (id == R.id.iv_forward) {
            stopPlayMusic();
            return;
        }
        if (id == R.id.imgv_play_mode) {
            int playMode = Music.getPlayMode();
            if (playMode == 0) {
                i = 1;
            } else if (playMode == 1) {
                i = 2;
            } else if (playMode != 2) {
                i = playMode;
            }
            Music.setPlayMode(i);
            if (i == 1) {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_random);
                return;
            } else if (i == 2) {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_single);
                return;
            } else {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_circulation);
                return;
            }
        }
        if (id == R.id.imgv_last) {
            setMicroPhoneOff();
            changeSong(Music.getLastSong());
            if (Music.getSTATE() == 1) {
                getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
                rhyUIShow(true);
                return;
            }
            return;
        }
        if (id == R.id.imgv_play_pause) {
            playOrPause();
            return;
        }
        if (id == R.id.imgv_next) {
            setMicroPhoneOff();
            changeSong(Music.getNextSong());
            if (Music.getSTATE() == 1) {
                getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
                rhyUIShow(true);
                return;
            }
            return;
        }
        if (id == R.id.iv_play_list) {
            if (this.isPlayVisible) {
                songListGone();
            } else {
                songListVisible();
            }
        }
    }

    private void setMicroPhoneOff() {
        DataManager.getInstance().setMicrophoneEnable(false);
        AnimationDrawable animationDrawable = this.ivForwardAnim;
        if (animationDrawable != null) {
            animationDrawable.stop();
            getBinding().top.ivForward.clearAnimation();
            getBinding().top.ivForward.setImageResource(R.mipmap.rhy_microphone);
        }
        rhyUIShow(false);
    }

    private void setMicroPhoneOn() {
        Music.setIsOpenedRhythm(false);
        DataManager.getInstance().setMicrophoneEnable(true);
        getBinding().top.ivForward.setImageResource(R.drawable.anim_microphone);
        AnimationDrawable animationDrawable = (AnimationDrawable) getBinding().top.ivForward.getDrawable();
        this.ivForwardAnim = animationDrawable;
        animationDrawable.start();
        MusicPlayer musicPlayer = this.musicPlayer;
        if (musicPlayer != null && musicPlayer.isPlaying()) {
            this.musicPlayer.pause();
        }
        getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_player);
        rhyUIShow(true);
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        DataManager.getInstance().setMicrophoneEnable(false);
        super.onBackPressed();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void songListVisible() {
        getBinding().rlBottom.setEnabled(false);
        this.isPlayVisible = true;
        getBinding().rlRhySelect.setVisibility(8);
        getBinding().ivAnim.setVisibility(8);
        fadeOut(getBinding().rlRhySelect);
        int screenHeight = ScreenUtils.getScreenHeight(this);
        getBinding().lstvSong.setVisibility(0);
        topMove(getBinding().rlSongList, -(screenHeight - ScreenUtils.dp2px(this, 215.0f)));
        this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.8
            @Override // java.lang.Runnable
            public void run() {
                RhythmActivity rhythmActivity = RhythmActivity.this;
                rhythmActivity.topMove1(((ActivityRhythm3Binding) rhythmActivity.getBinding()).llSongInfo, -140.0f);
            }
        }, 380L);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void songListGone() {
        getBinding().rlBottom.setEnabled(true);
        fadeIn(getBinding().rlRhySelect);
        this.isPlayVisible = false;
        getBinding().rlSongName.setVisibility(4);
        bottomMove(getBinding().rlSongList, -(ScreenUtils.getScreenHeight(this) - ScreenUtils.dp2px(this, 215.0f)));
        bottomMove1(getBinding().llSongInfo, 140.0f);
    }

    public static void fadeOut(View view) {
        AlphaAnimation alphaAnimation = new AlphaAnimation(1.0f, 0.0f);
        alphaAnimation.setDuration(400L);
        alphaAnimation.setRepeatCount(0);
        alphaAnimation.setRepeatMode(1);
        view.startAnimation(alphaAnimation);
    }

    public static void fadeIn(View view) {
        AlphaAnimation alphaAnimation = new AlphaAnimation(0.0f, 1.0f);
        alphaAnimation.setDuration(1500L);
        alphaAnimation.setRepeatCount(0);
        alphaAnimation.setRepeatMode(2);
        view.startAnimation(alphaAnimation);
    }

    private void topMove(final View view, float f) {
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, -f, 0.0f);
        translateAnimation.setDuration(1000L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.9
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(false);
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                view.clearAnimation();
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(true);
            }
        });
    }

    private void bottomMove(final View view, float f) {
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, 0.0f, -f);
        translateAnimation.setDuration(1000L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.10
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(false);
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(true);
                view.clearAnimation();
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).lstvSong.setVisibility(8);
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).rlRhySelect.setVisibility(0);
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).ivAnim.setVisibility(0);
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).llSongInfo.setVisibility(0);
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void topMove1(final View view, float f) {
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, 0.0f, (int) DensityUtil.dp2px(this, f));
        translateAnimation.setDuration(380L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(-1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.11
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                view.clearAnimation();
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).llSongInfo.setVisibility(4);
                ((ActivityRhythm3Binding) RhythmActivity.this.getBinding()).rlSongName.setVisibility(0);
            }
        });
    }

    private void bottomMove1(final View view, float f) {
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, -((int) DensityUtil.dp2px(this, f)), 0.0f);
        translateAnimation.setDuration(1000L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(-1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.test.RhythmActivity.12
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                view.clearAnimation();
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void rhyUIShow(boolean z) {
        LogUtil.d("显示律动UI：" + z);
        if (z) {
            this.handler.removeCallbacks(this.runnable);
            this.handler.postDelayed(this.runnable, 550L);
        } else {
            getBinding().rlRhyShow.setVisibility(8);
        }
    }
}