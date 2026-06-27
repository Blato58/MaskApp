package cn.com.heaton.shiningmask.ui.activity;

import android.animation.Animator;
import android.animation.ObjectAnimator;
import android.app.AlertDialog;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGattDescriptor;
import android.content.ComponentName;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.ServiceConnection;
import android.graphics.Bitmap;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.os.Message;
import android.text.TextUtils;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.animation.LinearInterpolator;
import android.widget.ListAdapter;
import android.widget.SeekBar;
import android.widget.Toast;
import androidx.activity.result.ActivityResult;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.IntentSenderRequest;
import androidx.activity.result.PickVisualMediaRequest;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.constraintlayout.motion.widget.Key;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;
import cn.com.heaton.shiningmask.BuildConfig;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.DataManager;
import cn.com.heaton.shiningmask.base.app.BleConfig;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.music.Music;
import cn.com.heaton.shiningmask.base.music.MusicListenter;
import cn.com.heaton.shiningmask.base.music.MusicPlayer;
import cn.com.heaton.shiningmask.base.music.VisualizerManager;
import cn.com.heaton.shiningmask.base.update.UpdateManager;
import cn.com.heaton.shiningmask.ble.HeartBeatDeviceFactory;
import cn.com.heaton.shiningmask.dao.DeviceDao;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import cn.com.heaton.shiningmask.dao.bean.Device;
import cn.com.heaton.shiningmask.databinding.ActivityConnect1Binding;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.sevice.FFTDataListenter;
import cn.com.heaton.shiningmask.sevice.MikeSevice;
import cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter;
import cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter;
import cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog;
import cn.com.heaton.shiningmask.ui.utils.AppUtils;
import cn.com.heaton.shiningmask.ui.utils.ByteUtils;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.PermissionPageUtils;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselZoomPostLayoutListener;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CenterScrollListener;
import com.cdbwsoft.library.AppConfig;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleListener;
import com.cdbwsoft.library.ble.BleManager;
import com.cdbwsoft.library.panchip.PanchipOtaManager;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.material.snackbar.Snackbar;
import com.google.android.play.core.appupdate.AppUpdateInfo;
import com.google.android.play.core.appupdate.AppUpdateManager;
import com.google.android.play.core.appupdate.AppUpdateManagerFactory;
import com.google.android.play.core.appupdate.AppUpdateOptions;
import com.google.android.play.core.install.InstallState;
import com.google.android.play.core.install.InstallStateUpdatedListener;
import com.yalantis.ucrop.UCrop;
import com.yanzhenjie.permission.Permission;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedTransferQueue;
import org.greenrobot.eventbus.EventBus;
import org.greenrobot.eventbus.Subscribe;
import org.greenrobot.greendao.query.WhereCondition;
import pub.devrel.easypermissions.AfterPermissionGranted;
import pub.devrel.easypermissions.EasyPermissions;

/* JADX INFO: loaded from: classes.dex */
public class ConnectActivity extends BaseActivity<ActivityConnect1Binding> implements EasyPermissions.PermissionCallbacks, View.OnClickListener {
    private static final int GALLERY_REQUEST_CODE = 110;
    private static final int GSP_OPEN = 1002;
    private static final int OTA_FAIL = 6;
    private static final int OTA_PROGRESS = 7;
    private static final int OTA_SUCCESS = 8;
    private static final int REQUEST_CODE_QRCODE_PERMISSIONS = 1111;
    private static final int REQUEST_ENABLE_BT = 1003;
    private static final int REQUEST_RECORD_AUDIO_PERMISSIONS = 3333;
    private static final int REQUEST_STORAGE_READ_ACCESS_PERMISSION = 1101;
    private static final int REQUEST_WRITE_PERMISSIONS = 2222;
    private static BleManager bleManager;
    private static String hardVersion;
    private static MikeSevice mikeSevice;
    private static MusicPlayer musicPlayer;
    private static BlockingQueue<byte[]> queue;
    private BleDevice bleDevice;
    private BleListener bleListener;
    private int curLight;
    private long curTime;
    private DeviceDao deviceDao;
    private FFTDataListenter fFTDataListenter;
    private boolean isConnectShow;
    private boolean isInitService;
    private DeviceAdapter mAdapter;
    private Uri mDestination;
    private PanchipOtaManager mPanchipOtaManager;
    private MusicListenterImpl musicListenter;
    private PicturesChooseDialog picturesChooseDialog;
    private ObjectAnimator refreshRotationAnimator;
    private ScanTimerTask scanTimerTask;
    private TextImageIconAdapter textImageIconAdapter;
    private Timer timer;
    private final int HANDLER_WHAT = 2;
    private final int HANDLER_WHAT_DISCONNECT = 3;
    private final int SCAN = 5;
    public List<BleDevice> mList = new ArrayList();
    private AppUpdateManager appUpdateManager = null;
    private Task<AppUpdateInfo> appUpdateInfoTask = null;
    ServiceConnection mikeServiceConnection = new ServiceConnection() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.2
        @Override // android.content.ServiceConnection
        public void onServiceConnected(ComponentName componentName, IBinder iBinder) {
            LogUtil.d("onServiceConnected");
            ConnectActivity.mikeSevice = ((MikeSevice.BinderImpl) iBinder).getService();
        }

        @Override // android.content.ServiceConnection
        public void onServiceDisconnected(ComponentName componentName) {
            LogUtil.d("onServiceDisconnected");
        }
    };
    InstallStateUpdatedListener listener = new InstallStateUpdatedListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.5
        @Override // com.google.android.play.core.listener.StateUpdatedListener
        public void onStateUpdate(InstallState installState) {
            LogUtil.d("LYY>>> onStateUpdate  " + installState.installStatus());
            installState.installStatus();
            if (installState.installStatus() == 11) {
                ConnectActivity.this.popupSnackbarForCompleteUpdate();
            }
        }
    };
    private final ActivityResultLauncher<IntentSenderRequest> activityResultLauncher = registerForActivityResult(new ActivityResultContracts.StartIntentSenderForResult(), new ActivityResultCallback<ActivityResult>() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.7
        @Override // androidx.activity.result.ActivityResultCallback
        public void onActivityResult(ActivityResult activityResult) {
            ConnectActivity.this.appUpdateManager.unregisterListener(ConnectActivity.this.listener);
            LogUtil.d("LYY>>> activityResultLauncher  " + activityResult.getResultCode());
            if (activityResult.getResultCode() != -1) {
                LogUtil.d("google 更新失败");
            }
        }
    });
    ServiceConnection serviceConnection = new ServiceConnection() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.9
        @Override // android.content.ServiceConnection
        public void onServiceDisconnected(ComponentName componentName) {
        }

        @Override // android.content.ServiceConnection
        public void onServiceConnected(ComponentName componentName, IBinder iBinder) {
            ConnectActivity.musicPlayer = ((MusicPlayer.BinderImpl) iBinder).getService();
            ConnectActivity.musicPlayer.registerMusicListenter(ConnectActivity.this.new MusicListenterImpl());
            try {
                ConnectActivity.this.initRythm();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    };
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.17
        @Override // java.lang.Runnable
        public void run() {
            ((ActivityConnect1Binding) ConnectActivity.this.getBinding()).ivRefresh.setEnabled(true);
            ((ActivityConnect1Binding) ConnectActivity.this.getBinding()).ivRefresh.setAlpha(1.0f);
            ConnectActivity.this.stopRefreshAnimator();
        }
    };
    private Handler handler = new Handler(Looper.getMainLooper()) { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.19
        @Override // android.os.Handler
        public void handleMessage(Message message) {
            super.handleMessage(message);
            int i = message.what;
            if (i == 2) {
                ConnectActivity.this.mAdapter.setList(ConnectActivity.this.mList);
                ConnectActivity.this.mAdapter.notifyDataSetChanged();
            } else if (i != 3) {
                if (i != 5) {
                    return;
                }
                ConnectActivity.this.startScan();
            } else {
                ConnectActivity.this.dismissProgressDialog();
                ConnectActivity.this.mAdapter.setList(ConnectActivity.this.mList);
                ConnectActivity.this.mAdapter.notifyDataSetChanged();
            }
        }
    };
    ActivityResultLauncher<PickVisualMediaRequest> pickMedia = registerForActivityResult(new ActivityResultContracts.PickVisualMedia(), new ActivityResultCallback() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity$$ExternalSyntheticLambda0
        @Override // androidx.activity.result.ActivityResultCallback
        public final void onActivityResult(Object obj) {
            this.f$0.lambda$new$0((Uri) obj);
        }
    });

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsGranted(int i, List<String> list) {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityConnect1Binding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityConnect1Binding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivSetting.setOnClickListener(this);
        getBinding().ivDeviceConnect.setOnClickListener(this);
        getBinding().rlDeviceConnect.setOnClickListener(this);
        getBinding().ivRefresh.setOnClickListener(this);
        EventBus.getDefault().register(this);
        initRecyclerView(getBinding().listHorizontalMenu, new CarouselLayoutManager(0, true));
        this.mAdapter = new DeviceAdapter(this, null);
        getBinding().lvDevice.setAdapter((ListAdapter) this.mAdapter);
        this.mAdapter.setOnlistener(new DeviceAdapter.OnConnectListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.1
            @Override // cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.OnConnectListener
            public void onLongClick(BleDevice bleDevice) {
            }

            @Override // cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.OnConnectListener
            public void rename(BleDevice bleDevice) {
            }

            @Override // cn.com.heaton.shiningmask.ui.adapter.DeviceAdapter.OnConnectListener
            public void connect(BleDevice bleDevice) {
                if (!bleDevice.isConnected()) {
                    ConnectActivity.this.stopScan();
                    ConnectActivity.this.connectDevice(bleDevice, true);
                } else {
                    ConnectActivity.this.saveDevice(bleDevice, false);
                    ConnectActivity.this.stopScan();
                    ConnectActivity.this.disconnectDevice(bleDevice);
                }
            }
        });
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.deviceDao = App.getDaoSession().getDeviceDao();
        this.bleListener = new BleListenerImpl();
        try {
            if (!BuildConfig.FLAVOR.equals(AppUtils.getAppMetaData(getApplicationContext(), AppConfig.META_CHANNEL))) {
                new UpdateManager(this).versionUpdate();
            } else {
                googleChannelUpdate();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        bindMikeService();
        bindMusicService();
        queue = new LinkedTransferQueue();
        new Thread(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.3
            @Override // java.lang.Runnable
            public void run() {
                while (true) {
                    try {
                        byte[] bArr = (byte[]) ConnectActivity.queue.take();
                        List<BleDevice> deviceList = App.getAppData().getDeviceList();
                        for (int i = 0; i < deviceList.size(); i++) {
                            deviceList.get(i).writeCharacteristicBy3(bArr);
                        }
                    } catch (Exception e2) {
                        e2.printStackTrace();
                    }
                }
            }
        }).start();
        requestPermissions();
    }

    private void googleChannelUpdate() {
        AppUpdateManager appUpdateManager = this.appUpdateManager;
        if (appUpdateManager != null) {
            appUpdateManager.unregisterListener(this.listener);
        }
        LogUtil.d("LYY>>> google upApp");
        AppUpdateManager appUpdateManagerCreate = AppUpdateManagerFactory.create(getApplicationContext());
        this.appUpdateManager = appUpdateManagerCreate;
        this.appUpdateInfoTask = appUpdateManagerCreate.getAppUpdateInfo();
        this.appUpdateManager.registerListener(this.listener);
        this.appUpdateInfoTask.addOnSuccessListener(new OnSuccessListener<AppUpdateInfo>() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.4
            @Override // com.google.android.gms.tasks.OnSuccessListener
            public void onSuccess(AppUpdateInfo appUpdateInfo) {
                LogUtil.d("LYY>>> UpdateAvailability ---- " + appUpdateInfo.updateAvailability());
                if (appUpdateInfo.updateAvailability() == 2 && appUpdateInfo.isUpdateTypeAllowed(1)) {
                    ConnectActivity.this.appUpdateManager.startUpdateFlowForResult(appUpdateInfo, ConnectActivity.this.activityResultLauncher, AppUpdateOptions.newBuilder(1).build());
                }
            }
        });
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void popupSnackbarForCompleteUpdate() {
        Snackbar.make(findViewById(R.id.rl_root), "An update has just been downloaded.", -2).setAction("RESTART", new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.6
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                if (ConnectActivity.this.appUpdateManager != null) {
                    ConnectActivity.this.appUpdateManager.completeUpdate();
                }
            }
        }).setActionTextColor(ContextCompat.getColor(this, R.color.text_color)).show();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
        getBinding().sbColour.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.8
            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStartTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onStopTrackingTouch(SeekBar seekBar) {
            }

            @Override // android.widget.SeekBar.OnSeekBarChangeListener
            public void onProgressChanged(SeekBar seekBar, int i, boolean z) {
                ((ActivityConnect1Binding) ConnectActivity.this.getBinding()).tvProgressLight.setText(i + "%");
                ConnectActivity.this.curLight = i;
                DataManager.getInstance().setCurLight(ConnectActivity.this.curLight);
                ConnectActivity connectActivity = ConnectActivity.this;
                connectActivity.sendLight(connectActivity.curLight);
            }
        });
    }

    public void sendLight(int i) {
        if (i == 0) {
            i = 1;
        }
        byte[] light = Agreement.getLight(i);
        LogUtil.e("sendLight:" + i);
        byte[] encryptData = Agreement.getEncryptData(light);
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i2 = 0; i2 < deviceList.size(); i2++) {
            deviceList.get(i2).writeCharacteristic(encryptData);
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    public void onRequestPermissionsResult(int i, String[] strArr, int[] iArr) {
        super.onRequestPermissionsResult(i, strArr, iArr);
        EasyPermissions.onRequestPermissionsResult(i, strArr, iArr, this);
    }

    @Override // pub.devrel.easypermissions.EasyPermissions.PermissionCallbacks
    public void onPermissionsDenied(int i, List<String> list) {
        if (!hasPermissionsFineLocation()) {
            finish();
        } else {
            permissionOpen();
        }
    }

    @AfterPermissionGranted(REQUEST_CODE_QRCODE_PERMISSIONS)
    private void requestPermissions() {
        if (Build.VERSION.SDK_INT >= 31) {
            String[] strArr = {"android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT", Permission.ACCESS_FINE_LOCATION};
            if (!EasyPermissions.hasPermissions(this, strArr)) {
                EasyPermissions.requestPermissions(this, getResources().getString(R.string.request_permission), REQUEST_CODE_QRCODE_PERMISSIONS, strArr);
                return;
            } else {
                initBLE();
                return;
            }
        }
        String[] strArr2 = {Permission.ACCESS_FINE_LOCATION};
        if (!EasyPermissions.hasPermissions(this, strArr2)) {
            EasyPermissions.requestPermissions(this, getResources().getString(R.string.request_permission), REQUEST_CODE_QRCODE_PERMISSIONS, strArr2);
        } else {
            initBLE();
        }
    }

    private void bindMikeService() {
        bindService(new Intent(this, (Class<?>) MikeSevice.class), this.mikeServiceConnection, 1);
        this.musicListenter = new MusicListenterImpl();
    }

    private void bindMusicService() {
        this.isInitService = true;
        bindService(new Intent(this, (Class<?>) MusicPlayer.class), this.serviceConnection, 1);
        this.musicListenter = new MusicListenterImpl();
    }

    @Override // androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onStart() {
        super.onStart();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void initRythm() {
        this.fFTDataListenter = new FFTDataListenter() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.10
            @Override // cn.com.heaton.shiningmask.sevice.FFTDataListenter
            public void onStart(byte[] bArr) {
                if (ConnectActivity.musicPlayer == null || !ConnectActivity.musicPlayer.isPlaying() || DataManager.getInstance().isMicrophoneEnable()) {
                    return;
                }
                int curSelectRhythmMode = DataManager.getInstance().getCurSelectRhythmMode();
                LogUtil.d("发送音乐律动：" + ByteUtils.binaryToHexString(bArr));
                ConnectActivity.this.sendRhythmData((byte) curSelectRhythmMode, bArr);
            }
        };
    }

    @Subscribe
    public void binFFtDataListenter(String str) {
        LogUtil.d("========BIND_FFT=========");
        if (hasPermissions() && str.equals(C.MAIN_EVENT.BIND_FFT) && VisualizerManager.getInstance() != null) {
            VisualizerManager.getInstance().setFftDataListenter(this.fFTDataListenter);
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

    private boolean hasPermissionsFineLocation() {
        String[] strArr = {Permission.ACCESS_FINE_LOCATION};
        if (Build.VERSION.SDK_INT >= 31) {
            strArr = new String[]{"android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT", Permission.ACCESS_FINE_LOCATION};
        }
        return EasyPermissions.hasPermissions(this, strArr);
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

    public static MusicPlayer getMusicPlayer() {
        return musicPlayer;
    }

    public static MikeSevice getMikeSevice() {
        return mikeSevice;
    }

    public class MusicListenterImpl extends MusicListenter {
        public MusicListenterImpl() {
        }

        @Override // cn.com.heaton.shiningmask.base.music.MusicListenter
        public void onStart() {
            super.onStart();
            Music.setSTATE(1);
            LogUtil.d("开始音乐");
        }

        @Override // cn.com.heaton.shiningmask.base.music.MusicListenter
        public void onPause() {
            LogUtil.d("暂停音乐");
            Music.setSTATE(2);
        }

        @Override // cn.com.heaton.shiningmask.base.music.MusicListenter
        public void onCompletion(MediaPlayer mediaPlayer) {
            LogUtil.e("onCompletion");
            Music.setSTATE(0);
            try {
                int playMode = Music.getPlayMode();
                if (playMode == 1) {
                    if (Music.getRandomSong() != null) {
                        ConnectActivity.musicPlayer.play(Music.getRandomSong().getFileUrl());
                    }
                } else if (playMode == 2) {
                    if (Music.getCurrentSong() != null) {
                        ConnectActivity.musicPlayer.rePlay(Music.getCurrentSong().getFileUrl());
                    }
                } else if (Music.getNextSong() != null) {
                    ConnectActivity.musicPlayer.play(Music.getNextSong().getFileUrl());
                }
            } catch (IOException e) {
                e.printStackTrace();
            } catch (Exception e2) {
                e2.printStackTrace();
            }
        }
    }

    public void initBLE() {
        LogUtil.d("初始化蓝牙");
        checkBLEStatus();
        bleManager = App.getInstance().getBleManager(this.bleListener, new HeartBeatDeviceFactory(this.mActivity));
        this.mPanchipOtaManager = PanchipOtaManager.getInstance();
    }

    private void checkBLEStatus() {
        if (!getPackageManager().hasSystemFeature("android.hardware.bluetooth_le")) {
            Toast.makeText(this, getString(R.string.ble_not_open), 0).show();
            if (this.serviceConnection != null) {
                MusicPlayer musicPlayer2 = musicPlayer;
                if (musicPlayer2 != null) {
                    musicPlayer2.release();
                }
                unbindService(this.serviceConnection);
            }
            finish();
            return;
        }
        BluetoothAdapter defaultAdapter = BluetoothAdapter.getDefaultAdapter();
        if (defaultAdapter != null) {
            if (!defaultAdapter.isEnabled()) {
                startActivityForResult(new Intent("android.bluetooth.adapter.action.REQUEST_ENABLE"), 1003);
            } else if (AppUtils.isGpsOpen(this)) {
                initScanTimer();
            } else {
                if (DataManager.getInstance().isShowGPSDialog()) {
                    return;
                }
                gpsOpen();
            }
        }
    }

    private void initScanTimer() {
        if (this.timer == null) {
            this.timer = new Timer();
            ScanTimerTask scanTimerTask = new ScanTimerTask();
            this.scanTimerTask = scanTimerTask;
            this.timer.schedule(scanTimerTask, 1000L, 10000L);
        }
    }

    class ScanTimerTask extends TimerTask {
        ScanTimerTask() {
        }

        @Override // java.util.TimerTask, java.lang.Runnable
        public void run() {
            Message message = new Message();
            message.what = 5;
            ConnectActivity.this.handler.sendMessage(message);
        }
    }

    public void cancelTimerTask1() {
        ScanTimerTask scanTimerTask = this.scanTimerTask;
        if (scanTimerTask != null) {
            scanTimerTask.cancel();
            this.handler.removeCallbacks(this.scanTimerTask);
        }
        Timer timer = this.timer;
        if (timer != null) {
            timer.cancel();
            this.timer.purge();
            this.timer = null;
        }
    }

    protected void permissionOpen() {
        new AlertDialog.Builder(this).setCancelable(false).setTitle(getString(com.cdbwsoft.library.R.string.tip)).setMessage(getResources().getString(R.string.tip2)).setNegativeButton(getString(R.string.btn_cancel), new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.12
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
            }
        }).setPositiveButton(getString(R.string.btn_ok), new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.11
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
                ConnectActivity connectActivity = ConnectActivity.this;
                new PermissionPageUtils(connectActivity, AppUtils.getPackageName(connectActivity)).jumpPermissionPage();
            }
        }).create().show();
    }

    protected void gpsOpen() {
        LogUtil.d("===打开GPS");
        DataManager.getInstance().setShowGPSDialog(true);
        if (AppUtils.isGpsOpen(this)) {
            return;
        }
        new AlertDialog.Builder(this).setCancelable(false).setTitle(com.cdbwsoft.library.R.string.tip).setMessage(R.string.gps_tip).setNegativeButton(R.string.btn_cancel, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.14
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
            }
        }).setPositiveButton(R.string.btn_ok, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.13
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                ConnectActivity.this.startActivityForResult(new Intent("android.settings.LOCATION_SOURCE_SETTINGS"), 1002);
                dialogInterface.dismiss();
            }
        }).create().show();
    }

    public void initRecyclerView(RecyclerView recyclerView, CarouselLayoutManager carouselLayoutManager) {
        ArrayList arrayList = new ArrayList();
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_text));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_image));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_diy));
        arrayList.add(Integer.valueOf(R.mipmap.main_menu_music));
        this.textImageIconAdapter = new TextImageIconAdapter(this, R.layout.item_image, arrayList);
        carouselLayoutManager.setPostLayoutListener(new CarouselZoomPostLayoutListener());
        carouselLayoutManager.setMaxVisibleItems(1);
        recyclerView.setLayoutManager(carouselLayoutManager);
        recyclerView.setHasFixedSize(true);
        recyclerView.setAdapter(this.textImageIconAdapter);
        this.textImageIconAdapter.setOniClickListener(new TextImageIconAdapter.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.15
            @Override // cn.com.heaton.shiningmask.ui.adapter.TextImageIconAdapter.OnItemClickListener
            public void onClick(int i) {
                LogUtil.d("index:" + i);
                if (ClickFilter.filter()) {
                    return;
                }
                ConnectActivity.this.intentActivity(i);
            }
        });
        recyclerView.addOnScrollListener(new CenterScrollListener());
        carouselLayoutManager.addOnItemSelectionListener(new CarouselLayoutManager.OnCenterItemSelectionListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.16
            @Override // cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.OnCenterItemSelectionListener
            public void onCenterItemChanged(int i) {
                LogUtil.d("adapterPosition:" + i);
            }
        });
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == R.id.iv_setting) {
            startActivity(new Intent(this, (Class<?>) SettingActivity.class));
            return;
        }
        if (id == R.id.iv_device_connect) {
            getBinding().rlDeviceConnect.setVisibility(0);
            this.isConnectShow = true;
            startScan();
            return;
        }
        if (id == R.id.rl_device_connect) {
            getBinding().rlDeviceConnect.setVisibility(8);
            this.isConnectShow = false;
            stopRefreshAnimator();
            stopScan();
            return;
        }
        if (id == R.id.iv_refresh) {
            LogUtil.d("刷新扫描");
            getBinding().ivRefresh.setEnabled(false);
            refreshAnimator();
            this.mList.clear();
            initDevice();
            this.mAdapter.notifyDataSetChanged();
            startScan();
            this.handler.removeCallbacks(this.runnable);
            this.handler.postDelayed(this.runnable, 10000L);
        }
    }

    public void startScan() {
        BleManager bleManager2 = bleManager;
        if (bleManager2 != null) {
            bleManager2.scanDevice();
        }
    }

    public void stopScan() {
        stopRefreshAnimator();
        getBinding().ivRefresh.setEnabled(true);
        getBinding().ivRefresh.setAlpha(1.0f);
    }

    public void initDevice() {
        List<BleDevice> deviceList = App.getAppData().getDeviceList();
        for (int i = 0; i < deviceList.size(); i++) {
            this.mList.add(deviceList.get(i));
        }
    }

    private void refreshAnimator() {
        ObjectAnimator objectAnimatorOfFloat = ObjectAnimator.ofFloat(getBinding().ivRefresh, Key.ROTATION, 0.0f, 360.0f);
        this.refreshRotationAnimator = objectAnimatorOfFloat;
        objectAnimatorOfFloat.setDuration(1000L);
        this.refreshRotationAnimator.setRepeatCount(10);
        this.refreshRotationAnimator.setInterpolator(new LinearInterpolator());
        this.refreshRotationAnimator.addListener(new Animator.AnimatorListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.18
            @Override // android.animation.Animator.AnimatorListener
            public void onAnimationCancel(Animator animator) {
            }

            @Override // android.animation.Animator.AnimatorListener
            public void onAnimationRepeat(Animator animator) {
            }

            @Override // android.animation.Animator.AnimatorListener
            public void onAnimationStart(Animator animator) {
                LogUtil.e("开始动画：ivForward.setEnabled(false)");
                ((ActivityConnect1Binding) ConnectActivity.this.getBinding()).ivRefresh.setEnabled(false);
            }

            @Override // android.animation.Animator.AnimatorListener
            public void onAnimationEnd(Animator animator) {
                LogUtil.e("停止动画：ivForward.setEnabled(true)");
                ((ActivityConnect1Binding) ConnectActivity.this.getBinding()).ivRefresh.setEnabled(true);
            }
        });
        this.refreshRotationAnimator.start();
    }

    public void stopRefreshAnimator() {
        ObjectAnimator objectAnimator = this.refreshRotationAnimator;
        if (objectAnimator == null || !objectAnimator.isRunning()) {
            return;
        }
        this.refreshRotationAnimator.cancel();
        this.refreshRotationAnimator = null;
        getBinding().ivRefresh.setAlpha(1.0f);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void intentActivity(int i) {
        Intent intent = new Intent();
        if (i == 0) {
            intent.setClass(this.mContext, TextEditActivity.class);
            startActivity(intent);
            return;
        }
        if (i == 1) {
            intent.setClass(this.mContext, ImageActivity.class);
            startActivity(intent);
            return;
        }
        if (i != 2) {
            if (i != 3) {
                return;
            }
            requestAudioPermissions();
        } else {
            List<CropImage> listLoadAll = App.getDaoSession().getCropImageDao().loadAll();
            if (listLoadAll != null && listLoadAll.size() >= 20) {
                ToastUtil.showToast(getResources().getString(R.string.tip_image_count));
            } else {
                showPicturesChooseDialog();
            }
        }
    }

    private void showPicturesChooseDialog() {
        if (this.picturesChooseDialog == null) {
            PicturesChooseDialog picturesChooseDialog = new PicturesChooseDialog(this.mContext, R.style.dialog_clearimage);
            this.picturesChooseDialog = picturesChooseDialog;
            picturesChooseDialog.setResultListener(new PicturesChooseDialog.ResultListener() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.20
                @Override // cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog.ResultListener
                public void importImage() {
                    LogUtil.d("导入相册图片");
                    ConnectActivity.this.pickMedia.launch(new PickVisualMediaRequest.Builder().setMediaType(ActivityResultContracts.PickVisualMedia.ImageOnly.INSTANCE).build());
                }

                @Override // cn.com.heaton.shiningmask.ui.dialog.PicturesChooseDialog.ResultListener
                public void camera() {
                    LogUtil.d("拍照");
                    ConnectActivity.this.gotoCamare();
                }
            });
        }
        if (this.picturesChooseDialog.isShowing()) {
            return;
        }
        this.picturesChooseDialog.show();
    }

    /* JADX INFO: Access modifiers changed from: private */
    public /* synthetic */ void lambda$new$0(Uri uri) {
        this.picturesChooseDialog.dismiss();
        if (uri != null) {
            startCropActivity(uri);
        }
    }

    public void gotoCamare() {
        this.picturesChooseDialog.dismiss();
        CameraActivity.startMe(this.mActivity, 0);
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    protected void onActivityResult(int i, int i2, Intent intent) {
        super.onActivityResult(i, i2, intent);
        if (i != 110) {
            return;
        }
        this.picturesChooseDialog.dismiss();
        if (intent != null) {
            startCropActivity(intent.getData());
        }
    }

    public void startCropActivity(Uri uri) {
        LogUtil.d("=====source:");
        this.mDestination = Uri.fromFile(new File(getCacheDir(), "cropImage" + System.currentTimeMillis() + ".png"));
        UCrop.Options options = new UCrop.Options();
        options.setToolbarColor(ContextCompat.getColor(this.mActivity, pub.devrel.easypermissions.R.color.colorPrimary));
        options.setStatusBarColor(ContextCompat.getColor(this.mActivity, pub.devrel.easypermissions.R.color.colorPrimaryDark));
        options.setCropFrameColor(0);
        options.setShowCropGrid(false);
        options.setHideBottomControls(true);
        options.setCompressionFormat(Bitmap.CompressFormat.PNG);
        options.setCompressionQuality(100);
        UCrop.of(uri, this.mDestination).withAspectRatio(46.0f, 58.0f).withMaxResultSize(46, 58).withOptions(options).start(this.mActivity, UCropActivity.class, 0);
    }

    public class BleListenerImpl extends BleListener {
        public BleListenerImpl() {
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public boolean filterDevice(BluetoothDevice bluetoothDevice, int i, byte[] bArr) {
            boolean z;
            try {
                z = true;
            } catch (Exception e) {
                e.printStackTrace();
            }
            if (BleConfig.matchProduct(bArr) == -1) {
                return true;
            }
            if (i != 0 && bluetoothDevice != null) {
                String address = bluetoothDevice.getAddress();
                String name = bluetoothDevice.getName();
                if (address != null && !address.isEmpty() && name != null && !name.isEmpty()) {
                    BleDevice bleDevice = null;
                    int i2 = 0;
                    while (true) {
                        if (i2 >= ConnectActivity.this.mList.size()) {
                            z = false;
                            break;
                        }
                        bleDevice = ConnectActivity.this.mList.get(i2);
                        if (bleDevice != null && address.equals(bleDevice.getBleAddress())) {
                            break;
                        }
                        i2++;
                    }
                    BleDevice bleDevice2 = new BleDevice(ConnectActivity.bleManager, address, name);
                    if (!z) {
                        ConnectActivity.this.mList.add(bleDevice2);
                        if (ConnectActivity.this.handler != null) {
                            Message messageObtainMessage = ConnectActivity.this.handler.obtainMessage();
                            messageObtainMessage.what = 2;
                            ConnectActivity.this.handler.sendMessage(messageObtainMessage);
                        }
                    } else if (ConnectActivity.this.isReConnect(bleDevice) && System.currentTimeMillis() - ConnectActivity.this.curTime > 2000) {
                        ConnectActivity.this.curTime = System.currentTimeMillis();
                        LogUtil.d("==重连2：" + bleDevice.getBleAddress());
                        ConnectActivity.this.connectDevice(bleDevice, false);
                    }
                    return false;
                }
            }
            return false;
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onRead(BleDevice bleDevice, byte[] bArr) {
            try {
                String str = new String(bArr);
                LogUtil.e("===onRead===".concat(str));
                if (str.contains("-")) {
                    String[] strArrSplit = str.split("-");
                    if (strArrSplit.length < 2 || Integer.parseInt(strArrSplit[1]) != 1 || strArrSplit.length < 3) {
                        return;
                    }
                    Integer.parseInt(strArrSplit[2]);
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onChanged(BleDevice bleDevice, byte[] bArr) {
            super.onChanged(bleDevice, bArr);
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onConnectionChanged(final BleDevice bleDevice) {
            super.onConnectionChanged(bleDevice);
            if (bleDevice == null || !bleDevice.isDisConnect()) {
                return;
            }
            LogUtil.d("===连接断开");
            ConnectActivity.this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.BleListenerImpl.1
                @Override // java.lang.Runnable
                public void run() {
                    List<BleDevice> deviceList = App.getAppData().getDeviceList();
                    for (int i = 0; i < deviceList.size(); i++) {
                        deviceList.remove(bleDevice);
                    }
                    ConnectActivity.this.updateUI(bleDevice);
                    Message messageObtainMessage = ConnectActivity.this.handler.obtainMessage();
                    messageObtainMessage.what = 3;
                    ConnectActivity.this.handler.sendMessage(messageObtainMessage);
                }
            }, 1000L);
        }

        @Override // com.cdbwsoft.library.ble.BleListener
        public void onReady(final BleDevice bleDevice, BluetoothGattDescriptor bluetoothGattDescriptor) {
            super.onReady(bleDevice, bluetoothGattDescriptor);
            LogUtil.d("LYY>>>=====bleDevice连接成功:" + bleDevice + "   读取硬件版本号是否成功：" + bleDevice.readCharacteristicByRead());
            SPUtils.put(ConnectActivity.this, "isConnectFlag", true);
            ConnectActivity.this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.BleListenerImpl.2
                @Override // java.lang.Runnable
                public void run() {
                    List<BleDevice> deviceList = App.getAppData().getDeviceList();
                    ConnectActivity.this.dismissProgressDialog();
                    ConnectActivity.this.updateUI(bleDevice);
                    int i = 0;
                    for (int i2 = 0; i2 < deviceList.size(); i2++) {
                        if (bleDevice.getBleAddress().equals(deviceList.get(i2).getBleAddress())) {
                            return;
                        }
                    }
                    deviceList.add(bleDevice);
                    ConnectActivity.this.saveDevice(bleDevice, true);
                    if (bleDevice.isConnected()) {
                        while (true) {
                            if (i >= deviceList.size()) {
                                break;
                            }
                            if (bleDevice.getBleAddress().equals(deviceList.get(i).getBleAddress())) {
                                deviceList.set(i, bleDevice);
                                LogUtil.d("==更新蓝牙设备");
                                break;
                            }
                            i++;
                        }
                    }
                    ConnectActivity.hardVersion = bleDevice.getHardVersion();
                    ConnectActivity.this.mPanchipOtaManager.setBleDevice(bleDevice);
                    ConnectActivity.this.handler.postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.BleListenerImpl.2.1
                        @Override // java.lang.Runnable
                        public void run() {
                            ConnectActivity.this.mPanchipOtaManager.setMtuSize1(bleDevice, 100);
                        }
                    }, 500L);
                }
            }, 1000L);
        }
    }

    public void ota(BleDevice bleDevice) {
        this.bleDevice = bleDevice;
        LogUtil.e("ota");
        LogUtil.e("result:" + bleDevice.readCharacteristicByRead());
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void saveDevice(BleDevice bleDevice, boolean z) {
        if (bleDevice != null) {
            try {
                Device deviceUnique = this.deviceDao.queryBuilder().where(DeviceDao.Properties.Mac.eq(bleDevice.getBleAddress()), new WhereCondition[0]).unique();
                if (deviceUnique != null && !TextUtils.isEmpty(deviceUnique.getMac())) {
                    deviceUnique.setDeviceName(bleDevice.getBleName());
                    deviceUnique.setIsReConnect(z);
                    deviceUnique.setMac(bleDevice.getBleAddress());
                    this.deviceDao.update(deviceUnique);
                    return;
                }
                Device device = new Device();
                device.setDeviceName(bleDevice.getBleName());
                device.setIsReConnect(z);
                device.setMac(bleDevice.getBleAddress());
                this.deviceDao.save(device);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    public static BleManager getBleManager() {
        return bleManager;
    }

    public static boolean isShowImageFlag() {
        LogUtil.d("LYY>>>获取到的硬件版本号：" + hardVersion);
        String str = hardVersion;
        return str == null || str.isEmpty() || "TR2111R004-01".compareTo(hardVersion) < 0;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public boolean isReConnect(BleDevice bleDevice) {
        Device deviceUnique;
        if (bleDevice != null) {
            try {
                if (!TextUtils.isEmpty(bleDevice.getBleAddress()) && (deviceUnique = this.deviceDao.queryBuilder().where(DeviceDao.Properties.Mac.eq(bleDevice.getBleAddress()), new WhereCondition[0]).unique()) != null) {
                    return deviceUnique.getIsReConnect();
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return false;
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void connectDevice(final BleDevice bleDevice, boolean z) {
        if (z) {
            showProgressDialog(this.mContext, getResources().getString(R.string.connecting));
        }
        LogUtil.d("====addDevice....");
        bleManager.addDevice(bleDevice);
        new Handler().postDelayed(new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.ConnectActivity.21
            @Override // java.lang.Runnable
            public void run() {
                LogUtil.d("====连接设备：" + bleDevice);
                ConnectActivity.bleManager.stopScan();
                ConnectActivity.bleManager.connect(bleDevice);
            }
        }, 1000L);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void disconnectDevice(BleDevice bleDevice) {
        bleDevice.disconnect();
        showProgressDialog(this.mActivity, getResources().getString(R.string.dissconnecting));
    }

    public void updateUI(BleDevice bleDevice) {
        LogUtil.d("===updateUI");
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < this.mList.size(); i++) {
            arrayList.add(this.mList.get(i).getBleAddress());
        }
        if (this.mAdapter == null || bleDevice == null) {
            return;
        }
        if (arrayList.contains(bleDevice.getBleAddress())) {
            LogUtil.d("当前列表中存在：" + bleDevice.getBleAddress());
            for (int i2 = 0; i2 < this.mList.size(); i2++) {
                if (bleDevice.getBleAddress().equals(this.mList.get(i2).getBleAddress())) {
                    LogUtil.d("更新扫描列表。。。" + bleDevice.toString());
                    this.mList.get(i2).setConnectionState(bleDevice.getConnectionState());
                }
            }
        } else if (bleDevice.isConnected()) {
            this.mList.add(bleDevice);
            LogUtil.d("添加设备");
            App.getAppData().getDeviceList().add(bleDevice);
        }
        this.mAdapter.setList(this.mList);
        this.mAdapter.notifyDataSetChanged();
    }

    @Override // androidx.appcompat.app.AppCompatActivity, android.app.Activity, android.view.KeyEvent.Callback
    public boolean onKeyDown(int i, KeyEvent keyEvent) {
        if (i == 4) {
            if (!this.isConnectShow) {
                moveTaskToBack(true);
            } else {
                getBinding().rlDeviceConnect.setVisibility(8);
                this.isConnectShow = false;
                stopRefreshAnimator();
                stopScan();
            }
            return true;
        }
        return super.onKeyDown(i, keyEvent);
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        if (!this.isConnectShow) {
            moveTaskToBack(true);
            return;
        }
        getBinding().rlDeviceConnect.setVisibility(8);
        this.isConnectShow = false;
        stopRefreshAnimator();
        stopScan();
    }

    @Subscribe
    public void stopRhy(String str) {
        if (str.equals(C.MAIN_EVENT.STOP_RHY)) {
            LogUtil.d("======停止音乐和律动");
            Music.setSTATE(2);
            MusicPlayer musicPlayer2 = musicPlayer;
            if (musicPlayer2 != null && musicPlayer2.isPlaying()) {
                musicPlayer.pause();
            }
            Music.setIsOpenedRhythm(false);
            return;
        }
        if (str.equals(C.MAIN_EVENT.STOP_RHY1)) {
            LogUtil.d("======停止音乐和律动1");
            Music.setSTATE(2);
            MusicPlayer musicPlayer3 = musicPlayer;
            if (musicPlayer3 != null && musicPlayer3.isPlaying()) {
                musicPlayer.stop();
                musicPlayer.unregisterMusicListenter(this.musicListenter);
            }
            Music.setIsOpenedRhythm(false);
            if (this.serviceConnection != null) {
                MusicPlayer musicPlayer4 = musicPlayer;
                if (musicPlayer4 != null) {
                    musicPlayer4.release();
                }
                if (this.isInitService) {
                    unbindService(this.serviceConnection);
                    this.serviceConnection = null;
                }
            }
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        LogUtil.d("========onDestroy============");
        try {
            if (this.isInitService && this.serviceConnection != null) {
                stopRhy(C.MAIN_EVENT.STOP_RHY);
                unbindService(this.serviceConnection);
                this.isInitService = false;
            }
            EventBus.getDefault().unregister(this);
            cancelTimerTask1();
            if (this.isInitService && this.mikeServiceConnection != null) {
                if (VisualizerManager.getInstance() != null) {
                    VisualizerManager.getInstance().removeFftDataListenter(this.fFTDataListenter);
                    VisualizerManager.getInstance().stop();
                    VisualizerManager.getInstance().clear();
                }
                mikeSevice.stop();
                unbindService(this.mikeServiceConnection);
            }
            AppUpdateManager appUpdateManager = this.appUpdateManager;
            if (appUpdateManager != null) {
                appUpdateManager.unregisterListener(this.listener);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @AfterPermissionGranted(REQUEST_RECORD_AUDIO_PERMISSIONS)
    private void requestAudioPermissions() {
        String[] strArr;
        LogUtil.e("requestAudioPermissions");
        if (Build.VERSION.SDK_INT >= 33) {
            strArr = new String[]{"android.permission.READ_MEDIA_AUDIO", Permission.RECORD_AUDIO};
        } else {
            strArr = new String[]{Permission.READ_EXTERNAL_STORAGE, Permission.RECORD_AUDIO};
        }
        if (hasAudioPermissions()) {
            Intent intent = new Intent();
            intent.setClass(this.mContext, RhythmActivity.class);
            startActivity(intent);
            return;
        }
        EasyPermissions.requestPermissions(this, getString(R.string.ble_read_permission_tip), REQUEST_RECORD_AUDIO_PERMISSIONS, strArr);
    }

    private boolean hasWritePermissions() {
        return EasyPermissions.hasPermissions(this, Permission.WRITE_EXTERNAL_STORAGE, Permission.READ_EXTERNAL_STORAGE);
    }

    private boolean hasAudioPermissions() {
        String[] strArr;
        if (Build.VERSION.SDK_INT >= 33) {
            strArr = new String[]{"android.permission.READ_MEDIA_AUDIO", Permission.RECORD_AUDIO};
        } else {
            strArr = new String[]{Permission.READ_EXTERNAL_STORAGE, Permission.RECORD_AUDIO};
        }
        return EasyPermissions.hasPermissions(this, strArr);
    }
}