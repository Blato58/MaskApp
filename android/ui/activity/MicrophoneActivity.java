package cn.com.heaton.shiningmask.ui.activity;

import android.graphics.Outline;
import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewOutlineProvider;
import android.widget.RelativeLayout;
import androidx.viewpager.widget.ViewPager;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.music.VisualizerUtil;
import cn.com.heaton.shiningmask.databinding.ActivityMicrophoneBinding;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.sevice.MikeListenter;
import cn.com.heaton.shiningmask.sevice.MikeSevice;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.RhythmDataUitls;
import cn.com.heaton.shiningmask.ui.utils.ScreenUtils;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ViewpagerAdapter;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ZoomOutPageTransformer;
import com.cdbwsoft.library.ble.BleDevice;
import com.yanzhenjie.permission.Permission;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedTransferQueue;
import pub.devrel.easypermissions.AfterPermissionGranted;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class MicrophoneActivity extends BaseActivity<ActivityMicrophoneBinding> implements ViewPager.OnPageChangeListener, EasyPermissions.PermissionCallbacks, View.OnClickListener {
    public static int MSG_MIKEPHONE_UPDATE_UI = 10001;
    private static final int REQUEST_RECORD_AUDIO_PERMISSIONS = 10006;
    private static BlockingQueue<byte[]> queue;
    private int curSelectPosition;
    long curTime;
    private DataManager dataManager;
    private boolean isStart;
    private byte[] mikeData;
    private MikeListenter mikeListenter;
    private MikeSevice mikeSevice;
    private ViewpagerAdapter textImageIconAdapter;
    private int curSelectMode = 0;
    private List<RhythmImage> imageList = new ArrayList();
    Runnable lineRunnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.1
        @Override // java.lang.Runnable
        public void run() {
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).rlRhyBg.setOutlineProvider(MicrophoneActivity.this.viewOutlineProvider);
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).rlRhyBg.setClipToOutline(true);
        }
    };
    final Handler handler = new Handler(Looper.getMainLooper()) { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.2
        @Override // android.os.Handler
        public void handleMessage(Message message) {
            super.handleMessage(message);
            if (message.what != MicrophoneActivity.MSG_MIKEPHONE_UPDATE_UI || MicrophoneActivity.this.mikeData == null) {
                return;
            }
            MicrophoneActivity microphoneActivity = MicrophoneActivity.this;
            microphoneActivity.setRhyData(microphoneActivity.mikeData);
        }
    };
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.5
        @Override // java.lang.Runnable
        public void run() {
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).ivRhybgTop.setVisibility(0);
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).ivRhyImageBg2.setVisibility(0);
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).rlRhyBg.setVisibility(0);
            ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).rlRhyShow.setVisibility(0);
        }
    };
    private ViewOutlineProvider viewOutlineProvider = new ViewOutlineProvider() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.6
        @Override // android.view.ViewOutlineProvider
        public void getOutline(View view, Outline outline) {
            outline.setOval(0, 0, view.getWidth(), view.getHeight());
        }
    };
    Runnable mikeRunnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.7
        @Override // java.lang.Runnable
        public void run() {
            MicrophoneActivity.this.mikeSevice.start();
        }
    };

    @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
    public void onPageScrolled(int i, float f, int i2) {
    }

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsGranted(int i, List<String> list) {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityMicrophoneBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityMicrophoneBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().top.ivForward.setOnClickListener(this);
        this.dataManager = DataManager.getInstance();
        getBinding().top.ivBack.setImageResource(R.mipmap.text_magic_back);
        getBinding().top.ivBack.setVisibility(8);
        getBinding().top.ivForward.setImageResource(R.mipmap.microphone_close);
        getBinding().top.ivForward.setVisibility(0);
        initRhyhmView();
        this.mikeListenter = new MikePhoneListenter();
        MikeSevice mikeSevice = ConnectActivity.getMikeSevice();
        this.mikeSevice = mikeSevice;
        mikeSevice.registerMusicListenter(this.mikeListenter);
        DataManager.getInstance().setMicrophoneEnable(true);
        initRhyUI();
        showCurRhyMode(0);
    }

    private void initRhyhmView() {
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
        getBinding().rlRhyBg.post(this.lineRunnable);
    }

    class MikePhoneListenter extends MikeListenter {
        MikePhoneListenter() {
        }

        @Override // cn.com.heaton.shiningmask.sevice.MikeListenter
        public void onStart(short[] sArr) {
            if (!DataManager.getInstance().isMicrophoneEnable() || System.currentTimeMillis() - MicrophoneActivity.this.curTime <= 120) {
                return;
            }
            byte[] waveFormData = VisualizerUtil.getWaveFormData(sArr);
            MicrophoneActivity microphoneActivity = MicrophoneActivity.this;
            microphoneActivity.sendRhythmData((byte) microphoneActivity.curSelectMode, waveFormData);
            MicrophoneActivity.this.mikeData = waveFormData;
            MicrophoneActivity.this.curTime = System.currentTimeMillis();
            MicrophoneActivity.this.handler.sendEmptyMessage(MicrophoneActivity.MSG_MIKEPHONE_UPDATE_UI);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.dataManager = DataManager.getInstance();
        this.curSelectMode = DataManager.getInstance().getCurSelectRhythmMode();
        requestRecordPermissions();
        queue = new LinkedTransferQueue();
        new Thread(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.3
            @Override // java.lang.Runnable
            public void run() {
                while (DataManager.getInstance().isMicrophoneEnable()) {
                    try {
                        byte[] bArr = (byte[]) MicrophoneActivity.queue.take();
                        List<BleDevice> deviceList = App.getAppData().getDeviceList();
                        for (int i = 0; i < deviceList.size(); i++) {
                            deviceList.get(i).writeCharacteristicBy3(bArr);
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            }
        }).start();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().rlRoot.setOnTouchListener(new View.OnTouchListener() { // from class: cn.com.heaton.shiningmask.ui.activity.MicrophoneActivity.4
            @Override // android.view.View.OnTouchListener
            public boolean onTouch(View view, MotionEvent motionEvent) {
                return ((ActivityMicrophoneBinding) MicrophoneActivity.this.getBinding()).vpRhyhm.dispatchTouchEvent(motionEvent);
            }
        });
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
        if (!ClickFilter.filter() && view.getId() == R.id.iv_forward) {
            finishCurActivity();
            finish();
        }
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        finishCurActivity();
        super.onBackPressed();
    }

    private void finishCurActivity() {
        DataManager.getInstance().setMicrophoneEnable(false);
        if (this.mikeSevice != null && hasPermissions()) {
            this.mikeSevice.stop();
        }
        this.handler.removeCallbacks(this.runnable);
        this.handler.removeCallbacks(this.mikeRunnable);
        getBinding().rlRhyBg.removeCallbacks(this.lineRunnable);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void sendRhythmData(byte b, byte[] bArr) {
        byte[] bArr2 = new byte[16];
        bArr2[0] = 15;
        bArr2[1] = b;
        System.arraycopy(bArr, 0, bArr2, 2, 12);
        try {
            queue.put(Agreement.getEncryptData(bArr2));
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
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
    }

    private void showCurRhyMode(int i) {
        rhyUIShow(DataManager.getInstance().isMicrophoneEnable());
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
        Log.d("ViewPage", " state:" + i);
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

    @AfterPermissionGranted(REQUEST_RECORD_AUDIO_PERMISSIONS)
    private void requestRecordPermissions() {
        LogUtil.e("requestBLEPermissions");
        String[] strArr = {Permission.RECORD_AUDIO};
        if (EasyPermissions.hasPermissions(this, strArr)) {
            LogUtil.e("初始化");
            this.handler.postDelayed(this.mikeRunnable, 1000L);
        } else {
            EasyPermissions.requestPermissions(this, getString(R.string.ble_read_permission_tip), REQUEST_RECORD_AUDIO_PERMISSIONS, strArr);
        }
    }

    private boolean hasPermissions() {
        return EasyPermissions.hasPermissions(this, Permission.RECORD_AUDIO);
    }
}