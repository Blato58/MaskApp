package cn.com.heaton.shiningmask.ui.activity;

import android.graphics.Outline;
import android.os.Build;
import android.os.CountDownTimer;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.text.TextUtils;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewOutlineProvider;
import android.view.animation.AlphaAnimation;
import android.view.animation.Animation;
import android.view.animation.DecelerateInterpolator;
import android.view.animation.TranslateAnimation;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.music.Music;
import cn.com.heaton.shiningmask.base.music.MusicListenter;
import cn.com.heaton.shiningmask.base.music.MusicPlayer;
import cn.com.heaton.shiningmask.base.music.Song;
import cn.com.heaton.shiningmask.base.music.VisualizerManager;
import cn.com.heaton.shiningmask.databinding.ActivityRhythmBinding;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import cn.com.heaton.shiningmask.sevice.FFTDataListenter;
import cn.com.heaton.shiningmask.ui.adapter.SongAdapter;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.DensityUtil;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.RhythmDataUitls;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ViewpagerAdapter;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ZoomOutPageTransformer;
import com.blankj.utilcode.constant.CacheConstants;
import com.yanzhenjie.permission.Permission;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import org.greenrobot.eventbus.EventBus;
import pub.devrel.easypermissions.AfterPermissionGranted;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class RhythmActivity extends BaseActivity<ActivityRhythmBinding> implements AdapterView.OnItemClickListener, SeekBar.OnSeekBarChangeListener, ViewPager.OnPageChangeListener, EasyPermissions.PermissionCallbacks, View.OnClickListener {
    private static final int MSG_MIKEPHONE_UPDATE_UI = 17;
    private static final int REQUEST_READ_PERMISSIONS = 1005;
    private static final int REQUEST_RECORD_AUDIO_PERMISSIONS = 1006;
    private int curSelectPosition;
    private DataManager dataManager;
    private FFTDataListenter fftDataListenter;
    private boolean isPlayVisible;
    private String minuteStr;
    MusicListenterImpl musicListenter;
    MusicPlayer musicPlayer;
    byte[] rhyData;
    private String secondStr;
    SongAdapter songAdapter;
    List<Song> songList;
    private ViewpagerAdapter textImageIconAdapter;
    private boolean rhythmStatus = true;
    private int curSelectMode = 0;
    private List<RhythmImage> imageList = new ArrayList();
    final Handler handler1 = new Handler(Looper.getMainLooper()) { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.3
        @Override // android.os.Handler
        public void handleMessage(Message message) {
            super.handleMessage(message);
            if (message.what != 17 || RhythmActivity.this.rhyData == null) {
                return;
            }
            RhythmActivity rhythmActivity = RhythmActivity.this;
            rhythmActivity.setRhyData(rhythmActivity.rhyData);
        }
    };
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.7
        @Override // java.lang.Runnable
        public void run() {
            ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).ivRhybgTop.setVisibility(0);
            ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).ivRhyImageBg2.setVisibility(0);
            ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlRhyBg.setVisibility(0);
            ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlRhyShow.setVisibility(0);
            for (int i = 0; i < RhythmActivity.this.imageList.size(); i++) {
                ((RhythmImage) RhythmActivity.this.imageList.get(i)).setShowImage(true);
            }
            ((RhythmImage) RhythmActivity.this.imageList.get(RhythmActivity.this.curSelectPosition)).setShowImage(false);
            RhythmActivity.this.textImageIconAdapter.notifyDataSetChanged();
        }
    };
    CountDownTimer playPositionTimer = new CountDownTimer(86400000, 1000) { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.8
        @Override // android.os.CountDownTimer
        public void onFinish() {
        }

        @Override // android.os.CountDownTimer
        public void onTick(long j) {
            ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).sbPlayTime.setProgress(RhythmActivity.this.musicPlayer.getCurrentPosition());
        }
    };
    Handler handler = new Handler();
    private ViewOutlineProvider viewOutlineProvider = new ViewOutlineProvider() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.14
        @Override // android.view.ViewOutlineProvider
        public void getOutline(View view, Outline outline) {
            LogUtil.d("裁剪成一个圆形");
            outline.setOval(0, 0, view.getWidth(), view.getHeight());
        }
    };

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageScrolled(int i, float f, int i2) {
    }

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsGranted(int i, List<String> list) {
    }

    @Override // android.widget.SeekBar.OnSeekBarChangeListener
    public void onStartTrackingTouch(SeekBar seekBar) {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityRhythmBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityRhythmBinding.inflate(layoutInflater);
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
        this.musicPlayer = ConnectActivity.getMusicPlayer();
        getBinding().top.ivBack.setImageResource(R.mipmap.text_back);
        getBinding().top.ivBack.setVisibility(0);
        getBinding().top.ivForward.setImageResource(R.mipmap.rhy_microphone);
        getBinding().top.ivForward.setVisibility(0);
        getBinding().sbPlayTime.setOnSeekBarChangeListener(this);
        initRhyUI();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.dataManager = DataManager.getInstance();
        this.curSelectMode = DataManager.getInstance().getCurSelectRhythmMode();
        getBinding().rhyledview1.setLayerType(1, null);
        getBinding().rhyledview1.setMode(0);
        getBinding().rhyledview1.setPointMargin(0);
        getBinding().rhyledview1.removeAllViews();
        getBinding().rhyledview1.init(52, 32);
        getBinding().rhyledview2.setLayerType(1, null);
        getBinding().rhyledview2.setMode(0);
        getBinding().rhyledview2.setPointMargin(0);
        getBinding().rhyledview2.removeAllViews();
        getBinding().rhyledview2.init(52, 32);
        getBinding().rlRhyBg.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.1
            @Override // java.lang.Runnable
            public void run() {
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlRhyBg.setOutlineProvider(RhythmActivity.this.viewOutlineProvider);
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlRhyBg.setClipToOutline(true);
            }
        }, 100L);
        this.fftDataListenter = new FFTDataListenter() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.2
            @Override // cn.com.heaton.shiningmask.sevice.FFTDataListenter
            public void onStart(byte[] bArr) {
                LogUtil.d("音乐律动数据：" + ByteUtils.binaryToHexString(bArr));
                if (!RhythmActivity.this.rhythmStatus || RhythmActivity.this.musicPlayer == null || !RhythmActivity.this.musicPlayer.isPlaying() || DataManager.getInstance().isMicrophoneEnable()) {
                    return;
                }
                RhythmActivity.this.rhyData = bArr;
                RhythmActivity.this.handler1.sendEmptyMessage(17);
            }
        };
        requestBLEPermissions();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().rlRoot.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.4
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                return ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).vpRhyhm.dispatchTouchEvent(motionEvent);
            }
        });
        getBinding().rlBottom.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.5
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
        getBinding().rlSongName.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.6
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

    private void showCurRhyMode(int i) {
        rhyUIShow(this.musicPlayer.isPlaying() || DataManager.getInstance().isMicrophoneEnable());
        this.dataManager.setCurSelectRhythmMode1(i);
        getBinding().rlRhyBg.setRotation(0.0f);
        getBinding().rhyledview1.setRotationX(0.0f);
        getBinding().rhyledview2.setRotationX(0.0f);
        if (i == 0) {
            getBinding().rlRhyBg.setRotation(0.0f);
            getBinding().rhyledview1.setRotationX(0.0f);
            getBinding().rhyledview2.setRotationX(180.0f);
            selectRhythmMode(1);
            return;
        }
        if (i == 1) {
            getBinding().rlRhyBg.setRotation(0.0f);
            getBinding().rhyledview1.setRotationX(0.0f);
            getBinding().rhyledview2.setRotationX(180.0f);
            selectRhythmMode(2);
            return;
        }
        if (i == 2) {
            getBinding().rlRhyBg.setRotation(0.0f);
            getBinding().rhyledview1.setRotationX(0.0f);
            getBinding().rhyledview2.setRotationX(180.0f);
            selectRhythmMode(3);
            return;
        }
        if (i == 3) {
            getBinding().rlRhyBg.setRotation(0.0f);
            getBinding().rhyledview1.setRotationX(180.0f);
            getBinding().rhyledview2.setRotationX(0.0f);
            selectRhythmMode(4);
            return;
        }
        if (i != 4) {
            return;
        }
        getBinding().rlRhyBg.setRotation(90.0f);
        getBinding().rhyledview1.setRotationX(0.0f);
        getBinding().rhyledview2.setRotationX(180.0f);
        selectRhythmMode(5);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void setRhyData(byte[] bArr) {
        int i = this.curSelectMode;
        if (i == 0) {
            List<Integer> rhyData1 = RhythmDataUitls.getRhyData1(bArr);
            getBinding().rhyledview1.updateRhythmUI(rhyData1);
            getBinding().rhyledview2.updateRhythmUI(rhyData1);
            return;
        }
        if (i == 1) {
            List<Integer> rhyData2 = RhythmDataUitls.getRhyData2(bArr);
            getBinding().rhyledview1.setRhyData2(rhyData2);
            getBinding().rhyledview2.setRhyData2(rhyData2);
            return;
        }
        if (i == 2) {
            List<Integer> rhyData3 = RhythmDataUitls.getRhyData3(bArr);
            getBinding().rhyledview1.setRhyData3(rhyData3);
            getBinding().rhyledview2.setRhyData3(rhyData3);
        } else if (i == 3) {
            List<Integer> rhyData4 = RhythmDataUitls.getRhyData4(bArr);
            getBinding().rhyledview1.setRhyData4(rhyData4);
            getBinding().rhyledview2.setRhyData4(rhyData4);
        } else {
            if (i != 4) {
                return;
            }
            List<Integer> rhyData5 = RhythmDataUitls.getRhyData5(bArr);
            getBinding().rhyledview1.setRhyData5(rhyData5);
            getBinding().rhyledview2.setRhyData5(rhyData5);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        stopPlayMusic();
        EventBus.getDefault().unregister(this);
        this.dataManager.setCurSelectRhythmMode(this.curSelectMode);
        CountDownTimer countDownTimer = this.playPositionTimer;
        if (countDownTimer != null) {
            countDownTimer.cancel();
        }
        if (!hasPermissions() || VisualizerManager.getInstance() == null) {
            return;
        }
        VisualizerManager.getInstance().removeFftDataListenter(this.fftDataListenter);
    }

    @Override // android.widget.AdapterView.OnItemClickListener
    public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
        Song song;
        if (ClickFilter.filter() || (song = Music.getSong(i)) == null) {
            return;
        }
        try {
            Music.setCurrentSong(song);
            this.musicPlayer.play(song.getFileUrl());
            getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
            openRhythm();
            if (VisualizerManager.getInstance() != null) {
                VisualizerManager.getInstance().setVisualezerEnable(true);
            }
            rhyUIShow(true);
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
            if (TextUtils.isEmpty(currentSong.getSinger()) || currentSong.getSinger().equals("<unknown>") || "未知歌手".equals(currentSong.getSinger())) {
                String string = getString(R.string.unkown_artist);
                LogUtil.d("歌手名===:" + string);
                getBinding().txvSinger1.setText(string);
            } else {
                getBinding().txvSinger1.setText(currentSong.getSinger());
            }
            int i2 = 0;
            while (true) {
                if (i2 >= this.songList.size()) {
                    break;
                }
                Song song = this.songList.get(i2);
                song.setState(0);
                if (song.getFileUrl().equals(currentSong.getFileUrl())) {
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
                RhythmActivity.this.songAdapter.notifyDataSetChanged();
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSongName.setText(currentSong.getTitle());
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSongName1.setText(currentSong.getTitle());
                if (!TextUtils.isEmpty(currentSong.getSinger()) && !currentSong.getSinger().equals("<unknown>") && !"未知歌手".equals(currentSong.getSinger())) {
                    ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSinger.setText(currentSong.getSinger());
                    ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSinger1.setText(currentSong.getSinger());
                } else {
                    String string = RhythmActivity.this.getString(R.string.unkown_artist);
                    ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSinger.setText(string);
                    ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvSinger1.setText(string);
                }
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
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).txvPlayDuration.setText(str + ":" + str2);
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).sbPlayTime.setMax(duration);
                RhythmActivity.this.playPositionTimer.start();
            }
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
                Song currentSong = Music.getCurrentSong();
                if (currentSong != null) {
                    this.musicPlayer.play(currentSong.getFileUrl());
                    getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
                    if (this.rhythmStatus) {
                        openRhythm();
                    }
                    rhyUIShow(true);
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
        if (VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().setVisualezerEnable(false);
        }
        Music.setIsOpenedRhythm(false);
        if (this.musicPlayer.isPlaying()) {
            this.musicPlayer.pause();
        }
        getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_player);
        Music.setIsOpenedRhythm(false);
        rhyUIShow(false);
    }

    private void startPalyMusic() {
        this.musicPlayer.start();
        getBinding().imgvPlayPause.setImageResource(R.mipmap.rhy_stop);
        if (VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().setVisualezerEnable(true);
        }
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
                if (VisualizerManager.getInstance() != null) {
                    VisualizerManager.getInstance().setVisualezerEnable(true);
                }
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

    private void selectRhythmMode(int i) {
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
        if (id == R.id.iv_back) {
            playOrPause();
            finish();
            return;
        }
        if (id == R.id.iv_forward) {
            stopPlayMusic();
            toActivity(MicrophoneActivity.class);
            return;
        }
        if (id == R.id.imgv_play_mode) {
            int playMode = Music.getPlayMode();
            if (playMode == 0) {
                playMode = 1;
            } else if (playMode == 1) {
                playMode = 2;
            } else if (playMode == 2) {
                playMode = 0;
            }
            Music.setPlayMode(playMode);
            if (playMode == 1) {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_random);
                return;
            } else if (playMode == 2) {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_single);
                return;
            } else {
                getBinding().imgvPlayMode.setImageResource(R.mipmap.rhy_circulation);
                return;
            }
        }
        if (id == R.id.imgv_last) {
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
                getBinding().ivPlayList.setImageResource(R.mipmap.iv_play_list);
                songListGone();
            } else {
                getBinding().ivPlayList.setImageResource(R.mipmap.music_list_btn);
                songListVisible();
            }
        }
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        super.onBackPressed();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void songListVisible() {
        this.isPlayVisible = true;
        getBinding().rlRhySelect.setVisibility(8);
        fadeOut(getBinding().rlRhySelect);
        int screenHeight = ScreenUtils.getScreenHeight(this);
        getBinding().lstvSong.setVisibility(0);
        topMove(getBinding().rlSongList, screenHeight - ScreenUtils.dp2px(this, -50.0f));
        this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.9
            @Override // java.lang.Runnable
            public void run() {
                RhythmActivity rhythmActivity = RhythmActivity.this;
                rhythmActivity.topMove1(((ActivityRhythmBinding) rhythmActivity.getBinding()).llSongInfo, ScreenUtils.dp2px(RhythmActivity.this, -5.0f));
            }
        }, 600L);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void songListGone() {
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
        LogUtil.d("topMove:" + f);
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, f, ScreenUtils.dp2px(this, 50.0f));
        translateAnimation.setDuration(1000L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.10
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

    private void bottomMove(final View view, float f) {
        TranslateAnimation translateAnimation = new TranslateAnimation(0.0f, 0.0f, 0.0f, -f);
        translateAnimation.setDuration(1000L);
        translateAnimation.setRepeatCount(0);
        translateAnimation.setRepeatMode(1);
        translateAnimation.setInterpolator(new DecelerateInterpolator());
        view.startAnimation(translateAnimation);
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.11
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(false);
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).ivPlayList.setEnabled(true);
                view.clearAnimation();
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).lstvSong.setVisibility(8);
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlRhySelect.setVisibility(0);
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).llSongInfo.setVisibility(0);
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
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.12
            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationRepeat(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationStart(Animation animation) {
            }

            @Override // android.view.animation.Animation.AnimationListener
            public void onAnimationEnd(Animation animation) {
                view.clearAnimation();
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).llSongInfo.setVisibility(4);
                ((ActivityRhythmBinding) RhythmActivity.this.getBinding()).rlSongName.setVisibility(0);
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
        translateAnimation.setAnimationListener(new Animation.AnimationListener() { // from class: cn.com.heaton.shiningmask.ui.activity.RhythmActivity.13
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

    private void rhyUIShow(boolean z) {
        LogUtil.d("显示律动UI：" + z);
        this.handler.removeCallbacks(this.runnable);
        if (z) {
            this.handler.postDelayed(this.runnable, 350L);
            return;
        }
        getBinding().rlRhyShow.setVisibility(0);
        getBinding().ivRhybgTop.setVisibility(8);
        getBinding().ivRhyImageBg2.setVisibility(8);
        getBinding().rlRhyBg.setVisibility(8);
        for (int i = 0; i < this.imageList.size(); i++) {
            this.imageList.get(i).setShowImage(true);
        }
        this.textImageIconAdapter.notifyDataSetChanged();
    }

    private void initRhyUI() {
        RhythmImage rhythmImage = new RhythmImage(R.mipmap.rhyhm_mode_bg1, true);
        RhythmImage rhythmImage2 = new RhythmImage(R.mipmap.rhyhm_mode_bg2, true);
        RhythmImage rhythmImage3 = new RhythmImage(R.mipmap.rhyhm_mode_bg3, true);
        RhythmImage rhythmImage4 = new RhythmImage(R.mipmap.rhyhm_mode_bg4, true);
        RhythmImage rhythmImage5 = new RhythmImage(R.mipmap.rhyhm_mode_bg5, true);
        this.imageList.add(rhythmImage);
        this.imageList.add(rhythmImage2);
        this.imageList.add(rhythmImage3);
        this.imageList.add(rhythmImage4);
        this.imageList.add(rhythmImage5);
        RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams) getBinding().rlRoot.getLayoutParams();
        layoutParams.width = ScreenUtils.getScreenWidth(this);
        getBinding().rlRoot.setLayoutParams(layoutParams);
        RelativeLayout.LayoutParams layoutParams2 = (RelativeLayout.LayoutParams) getBinding().vpRhyhm.getLayoutParams();
        layoutParams2.width = (int) (((double) ScreenUtils.getScreenWidth(this)) / 1.3d);
        layoutParams2.height = (int) (((double) ScreenUtils.getScreenWidth(this)) / 1.3d);
        getBinding().vpRhyhm.setLayoutParams(layoutParams2);
        this.textImageIconAdapter = new ViewpagerAdapter(this, getBinding().vpRhyhm, this.imageList);
        getBinding().vpRhyhm.setAdapter(this.textImageIconAdapter);
        getBinding().vpRhyhm.setPageTransformer(true, new ZoomOutPageTransformer());
        getBinding().vpRhyhm.setPageMargin(ScreenUtils.dp2px(this, -60.0f));
        getBinding().vpRhyhm.setCurrentItem(5000);
        getBinding().vpRhyhm.setOffscreenPageLimit(2);
        getBinding().vpRhyhm.addOnPageChangeListener(this);
    }

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageScrollStateChanged(int i) {
        if (i == 1 || i == 2) {
            Log.d("TAG", "开始滑动");
            rhyUIShow(false);
        } else if (i == 0) {
            Log.d("TAG", "停止");
            showCurRhyMode(this.curSelectPosition);
        }
    }

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageSelected(int i) {
        this.curSelectPosition = i % 5;
        Log.d("ViewPage", i + "");
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        super.onRequestPermissionsResult(i, strArr, iArr);
        EasyPermissions.onRequestPermissionsResult(i, strArr, iArr, this);
    }

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsDenied(int i, List<String> list) {
        if (hasPermissions()) {
            return;
        }
        finish();
    }

    @AfterPermissionGranted(1005)
    private void requestBLEPermissions() {
        String[] strArr;
        LogUtil.e("requestBLEPermissions");
        if (Build.VERSION.SDK_INT >= 33) {
            strArr = new String[]{"android.permission.READ_MEDIA_AUDIO", Permission.RECORD_AUDIO};
        } else {
            strArr = new String[]{Permission.READ_EXTERNAL_STORAGE, Permission.RECORD_AUDIO};
        }
        if (EasyPermissions.hasPermissions(this, strArr)) {
            LogUtil.e("初始化音乐播放器");
            initMusicList();
        } else {
            LogUtil.e("请打开蓝牙或存储相关权限");
            EasyPermissions.requestPermissions(this, getString(R.string.ble_read_permission_tip), 1005, strArr);
        }
    }

    private boolean hasPermissions() {
        String[] strArr;
        if (Build.VERSION.SDK_INT >= 33) {
            strArr = new String[]{"android.permission.READ_MEDIA_AUDIO", Permission.RECORD_AUDIO};
        } else {
            strArr = new String[]{Permission.READ_EXTERNAL_STORAGE, Permission.RECORD_AUDIO};
        }
        return EasyPermissions.hasPermissions(this, strArr);
    }

    @AfterPermissionGranted(1006)
    private void requestRecordPermissions() {
        LogUtil.e("requestBLEPermissions");
        String[] strArr = {Permission.READ_EXTERNAL_STORAGE, Permission.RECORD_AUDIO};
        if (EasyPermissions.hasPermissions(this, strArr)) {
            LogUtil.e("初始化音乐播放器");
        } else {
            EasyPermissions.requestPermissions(this, getString(R.string.ble_read_permission_tip), 1006, strArr);
        }
    }

    private void initMusicList() {
        EventBus.getDefault().post(C.MAIN_EVENT.BIND_FFT);
        if (VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().setFftDataListenter(this.fftDataListenter);
        }
        this.songList = this.musicPlayer.searchAndSortOrder("");
        for (int i = 0; i < this.songList.size(); i++) {
            LogUtil.d("" + this.songList.get(i).getFileName() + " " + this.songList.get(i).getSinger());
            String singer = this.songList.get(i).getSinger();
            if (TextUtils.isEmpty(singer) || singer.equals("<unknown>") || "未知歌手".equals(singer)) {
                LogUtil.d("歌手名===:" + getResources().getString(R.string.unkown_artist));
                this.songList.get(i).setSinger(singer);
            }
        }
        Music.setSongList(this.songList);
        this.songAdapter = new SongAdapter(this, this.songList);
        getBinding().lstvSong.setAdapter((ListAdapter) this.songAdapter);
        List<Song> list = this.songList;
        if (list != null && !list.isEmpty()) {
            Music.setCurrentSong(this.songList.get(0));
        }
        for (int i2 = 0; i2 < ((List) Objects.requireNonNull(this.songList)).size(); i2++) {
            this.songList.get(i2).setState(0);
        }
        this.songList = Music.getSongList();
        this.songAdapter = new SongAdapter(this, this.songList);
        getBinding().lstvSong.setAdapter((ListAdapter) this.songAdapter);
        getBinding().lstvSong.setOnItemClickListener(this);
        initMusicPlayer();
    }
}