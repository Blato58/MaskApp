package cn.com.heaton.shiningmask.ui.activity;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Matrix;
import android.hardware.Camera;
import android.media.ExifInterface;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import androidx.core.content.ContextCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.databinding.ActivityCamreLayoutBinding;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import cn.com.heaton.shiningmask.ui.widget.camera.CameraPreview;
import cn.com.heaton.shiningmask.ui.widget.camera.OverCameraView;
import cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils;
import com.alibaba.fastjson2.internal.asm.Opcodes;
import com.yalantis.ucrop.UCrop;
import com.yanzhenjie.permission.AndPermission;
import com.yanzhenjie.permission.Permission;
import java.io.File;
import java.io.IOException;
import org.greenrobot.eventbus.EventBus;
import org.greenrobot.eventbus.Subscribe;

/* JADX INFO: loaded from: classes.dex */
public class CameraActivity extends BaseActivity<ActivityCamreLayoutBinding> implements View.OnClickListener {
    private static final int BACK = 1;
    private static final int FRONT = 2;
    private static final String TAG = "CameraActivity";
    public static int flag;
    private byte[] imageData;
    private boolean isFoucing;
    private boolean isTakePhoto;
    private Camera mCamera;
    private Uri mDestination;
    private OverCameraView mOverCameraView;
    private Handler mHandler = new Handler();
    private int currentCameraType = 2;
    private Camera.AutoFocusCallback autoFocusCallback = new Camera.AutoFocusCallback() { // from class: cn.com.heaton.shiningmask.ui.activity.CameraActivity.3
        @Override // android.hardware.Camera.AutoFocusCallback
        public void onAutoFocus(boolean z, Camera camera) {
            CameraActivity.this.isFoucing = false;
            CameraActivity.this.mOverCameraView.setFoucuing(false);
            CameraActivity.this.mOverCameraView.disDrawTouchFocusRect();
            CameraActivity.this.mHandler.removeCallbacks(CameraActivity.this.runnable);
        }
    };
    Runnable runnable = new Runnable() { // from class: cn.com.heaton.shiningmask.ui.activity.CameraActivity.4
        @Override // java.lang.Runnable
        public void run() {
            LogUtil.d("自动聚焦超时,请调整合适的位置拍摄！");
            CameraActivity.this.isFoucing = false;
            CameraActivity.this.mOverCameraView.setFoucuing(false);
            CameraActivity.this.mOverCameraView.disDrawTouchFocusRect();
        }
    };

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
    }

    public static void startMe(final Activity activity, int i) {
        flag = i;
        Log.e(TAG, "启动拍照");
        if (Build.VERSION.SDK_INT >= 33) {
            PermissionUtils.applicationPermissions(activity, new PermissionUtils.PermissionListener() { // from class: cn.com.heaton.shiningmask.ui.activity.CameraActivity.1
                @Override // cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils.PermissionListener
                public void onSuccess(Context context) {
                    Log.e(CameraActivity.TAG, "拍照:");
                    activity.startActivity(new Intent(activity, (Class<?>) CameraActivity.class));
                }

                @Override // cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils.PermissionListener
                public void onFailed(Context context) {
                    if (AndPermission.hasAlwaysDeniedPermission(context, Permission.Group.CAMERA) && AndPermission.hasAlwaysDeniedPermission(context, Permission.Group.STORAGE)) {
                        AndPermission.with(context).runtime().setting().start();
                    }
                    ToastUtil.showToast(context.getString(R.string.tip2));
                }
            }, Permission.Group.CAMERA);
        } else {
            PermissionUtils.applicationPermissions(activity, new PermissionUtils.PermissionListener() { // from class: cn.com.heaton.shiningmask.ui.activity.CameraActivity.2
                @Override // cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils.PermissionListener
                public void onSuccess(Context context) {
                    Log.e(CameraActivity.TAG, "拍照:");
                    activity.startActivity(new Intent(activity, (Class<?>) CameraActivity.class));
                }

                @Override // cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils.PermissionListener
                public void onFailed(Context context) {
                    if (AndPermission.hasAlwaysDeniedPermission(context, Permission.Group.CAMERA) && AndPermission.hasAlwaysDeniedPermission(context, Permission.Group.STORAGE)) {
                        AndPermission.with(context).runtime().setting().start();
                    }
                    ToastUtil.showToast(context.getString(R.string.tip2));
                }
            }, Permission.Group.STORAGE, Permission.Group.CAMERA);
        }
    }

    @Override // android.app.Activity
    public boolean onTouchEvent(MotionEvent motionEvent) {
        LogUtil.d("isFoucing:" + this.isFoucing);
        if (motionEvent.getAction() == 0 && !this.isFoucing) {
            duijiao(motionEvent.getX(), motionEvent.getY());
        }
        return super.onTouchEvent(motionEvent);
    }

    private void takePhoto() {
        try {
            this.mCamera.takePicture(null, null, null, new Camera.PictureCallback() { // from class: cn.com.heaton.shiningmask.ui.activity.CameraActivity$$ExternalSyntheticLambda0
                @Override // android.hardware.Camera.PictureCallback
                public final void onPictureTaken(byte[] bArr, Camera camera) throws Throwable {
                    this.f$0.lambda$takePhoto$0(bArr, camera);
                }
            });
        } catch (RuntimeException e) {
            e.printStackTrace();
        } catch (Exception e2) {
            e2.printStackTrace();
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public /* synthetic */ void lambda$takePhoto$0(byte[] bArr, Camera camera) throws Throwable {
        this.imageData = bArr;
        this.mCamera.stopPreview();
        cancelDuijiao();
        this.mHandler.removeCallbacks(this.runnable);
        savePhoto();
    }

    private void cancleSavePhoto() {
        changeCamera();
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public ActivityCamreLayoutBinding inflateBinding(LayoutInflater layoutInflater) {
        return ActivityCamreLayoutBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public void initView() {
        getBinding().cancleButton.setOnClickListener(this);
        getBinding().takePhotoButton.setOnClickListener(this);
        getBinding().ivRight.setOnClickListener(this);
        EventBus.getDefault().register(this);
        this.mDestination = Uri.fromFile(new File(getCacheDir(), "cropImage" + System.currentTimeMillis() + ".png"));
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        if (this.mCamera == null) {
            int i = this.currentCameraType;
            if (i == 2) {
                this.mCamera = openCamera(1);
            } else if (i == 1) {
                this.mCamera = openCamera(2);
            }
        }
        CameraPreview cameraPreview = new CameraPreview(this, this.mCamera);
        this.mOverCameraView = new OverCameraView(this);
        getBinding().cameraPreviewLayout.removeAllViews();
        getBinding().cameraPreviewLayout.addView(cameraPreview);
        getBinding().cameraPreviewLayout.addView(this.mOverCameraView);
    }

    /* JADX WARN: Multi-variable type inference failed */
    /* JADX WARN: Removed duplicated region for block: B:34:0x00df  */
    /* JADX WARN: Removed duplicated region for block: B:55:? A[RETURN, SYNTHETIC] */
    /* JADX WARN: Type inference failed for: r0v12, types: [java.lang.String] */
    /* JADX WARN: Type inference failed for: r0v13 */
    /* JADX WARN: Type inference failed for: r0v14, types: [java.lang.String] */
    /* JADX WARN: Type inference failed for: r0v16, types: [android.net.Uri] */
    /* JADX WARN: Type inference failed for: r1v17, types: [java.lang.StringBuilder] */
    /* JADX WARN: Type inference failed for: r2v12, types: [java.lang.String] */
    /* JADX WARN: Type inference failed for: r2v18 */
    /* JADX WARN: Type inference failed for: r2v20 */
    /* JADX WARN: Type inference failed for: r2v5 */
    /* JADX WARN: Type inference failed for: r2v6 */
    /* JADX WARN: Type inference failed for: r2v7 */
    /* JADX WARN: Type inference failed for: r2v8 */
    /* JADX WARN: Type inference failed for: r2v9, types: [int] */
    /* JADX WARN: Type inference failed for: r5v12 */
    /* JADX WARN: Type inference failed for: r5v16 */
    /* JADX WARN: Type inference failed for: r5v17 */
    /* JADX WARN: Type inference failed for: r5v18 */
    /* JADX WARN: Type inference failed for: r5v2 */
    /* JADX WARN: Type inference failed for: r5v3, types: [java.io.FileOutputStream] */
    /* JADX WARN: Type inference failed for: r5v5 */
    /* JADX WARN: Type inference failed for: r5v9 */
    /* JADX WARN: Type inference failed for: r7v0, types: [cn.com.heaton.shiningmask.ui.activity.CameraActivity] */
    /*
        Code decompiled incorrectly, please refer to instructions dump.
        To view partially-correct code enable 'Show inconsistent code' option in preferences
    */
    private void savePhoto() throws java.lang.Throwable {
        /*
            Method dump skipped, instruction units count: 255
            To view this dump change 'Code comments level' option to 'DEBUG'
        */
        throw new UnsupportedOperationException("Method not decompiled: cn.com.heaton.shiningmask.ui.activity.CameraActivity.savePhoto():void");
    }

    private Camera openCamera(int i) {
        int numberOfCameras = Camera.getNumberOfCameras();
        Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
        int i2 = -1;
        int i3 = -1;
        for (int i4 = 0; i4 < numberOfCameras; i4++) {
            Camera.getCameraInfo(i4, cameraInfo);
            if (cameraInfo.facing == 1) {
                i2 = i4;
            } else if (cameraInfo.facing == 0) {
                i3 = i4;
            }
        }
        this.currentCameraType = i;
        if (i == 2 && i2 != -1) {
            return Camera.open(i2);
        }
        if (i != 1 || i3 == -1) {
            return null;
        }
        return Camera.open(i3);
    }

    private void changeCamera() {
        try {
            if (checkCamera()) {
                Camera camera = this.mCamera;
                if (camera != null) {
                    camera.stopPreview();
                    this.mCamera.release();
                }
                int i = this.currentCameraType;
                if (i == 2) {
                    this.mCamera = openCamera(1);
                } else if (i == 1) {
                    this.mCamera = openCamera(2);
                }
                CameraPreview cameraPreview = new CameraPreview(this, this.mCamera);
                this.mOverCameraView = new OverCameraView(this);
                getBinding().cameraPreviewLayout.removeAllViews();
                getBinding().cameraPreviewLayout.addView(cameraPreview);
                getBinding().cameraPreviewLayout.addView(this.mOverCameraView);
            }
        } catch (RuntimeException e) {
            e.printStackTrace();
        } catch (Exception e2) {
            e2.printStackTrace();
        }
    }

    private boolean checkCamera() {
        return getPackageManager().hasSystemFeature("android.hardware.camera.any");
    }

    @Override // android.app.Activity
    protected void onRestart() {
        super.onRestart();
        LogUtil.d("onRestart");
        startCamera();
    }

    @Override // androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onPause() {
        super.onPause();
        LogUtil.d("onPause");
        Camera camera = this.mCamera;
        if (camera != null) {
            camera.stopPreview();
            cancelDuijiao();
            this.mHandler.removeCallbacks(this.runnable);
        }
    }

    public void startCropActivity(Uri uri) {
        UCrop.Options options = new UCrop.Options();
        options.setToolbarColor(ContextCompat.getColor(this, pub.devrel.easypermissions.R.color.colorPrimary));
        options.setStatusBarColor(ContextCompat.getColor(this, pub.devrel.easypermissions.R.color.colorPrimaryDark));
        options.setCropFrameColor(0);
        options.setShowCropGrid(false);
        options.setHideBottomControls(true);
        options.setCompressionFormat(Bitmap.CompressFormat.JPEG);
        options.setCompressionQuality(100);
        UCrop.of(uri, this.mDestination).withAspectRatio(46.0f, 58.0f).withMaxResultSize(46, 58).withOptions(options).start(this, UCropActivity.class, flag);
    }

    private void duijiao(float f, float f2) {
        LogUtil.d("isTakePhoto:" + this.isTakePhoto);
        if (this.mCamera != null && !this.isTakePhoto) {
            LogUtil.d("对焦");
            this.isFoucing = true;
            this.mOverCameraView.setTouchFoucusRect(this.mCamera, this.autoFocusCallback, f, f2);
        }
        this.mHandler.postDelayed(this.runnable, 2000L);
    }

    private void cancelDuijiao() {
        OverCameraView overCameraView = this.mOverCameraView;
        if (overCameraView != null) {
            this.isFoucing = false;
            overCameraView.setFoucuing(false);
            this.mOverCameraView.disDrawTouchFocusRect();
        }
    }

    @Override // androidx.fragment.app.FragmentActivity, androidx.activity.ComponentActivity, android.app.Activity
    protected void onActivityResult(int i, int i2, Intent intent) {
        super.onActivityResult(i, i2, intent);
        LogUtil.d("返回resultCode：" + i2 + " requestCode:" + i);
        if (i != 10002) {
            return;
        }
        startCamera();
    }

    private void startCamera() {
        if (checkCamera()) {
            this.isFoucing = false;
            this.mCamera = openCamera(this.currentCameraType);
            CameraPreview cameraPreview = new CameraPreview(this, this.mCamera);
            this.mOverCameraView = new OverCameraView(this);
            getBinding().cameraPreviewLayout.removeAllViews();
            getBinding().cameraPreviewLayout.addView(cameraPreview);
            getBinding().cameraPreviewLayout.addView(this.mOverCameraView);
        }
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == R.id.cancle_button) {
            finish();
        } else if (id == R.id.take_photo_button) {
            takePhoto();
        } else if (id == R.id.iv_right) {
            changeCamera();
        }
    }

    @Subscribe
    public void finishCurActivity(String str) {
        if (C.MAIN_EVENT.FINISH_CAMERAACTIVITY.equals(str)) {
            LogUtil.d("FINISH_CAMERAACTIVITY");
            finish();
        }
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity, androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onDestroy() {
        super.onDestroy();
        cancelDuijiao();
        this.mHandler.removeCallbacks(this.runnable);
        EventBus.getDefault().unregister(this);
    }

    private int getBitmapDegree(String str) {
        try {
            int attributeInt = new ExifInterface(str).getAttributeInt(androidx.exifinterface.media.ExifInterface.TAG_ORIENTATION, 1);
            if (attributeInt == 3) {
                return Opcodes.GETFIELD;
            }
            if (attributeInt != 6) {
                return attributeInt != 8 ? 0 : 270;
            }
            return 90;
        } catch (IOException e) {
            e.printStackTrace();
            return 0;
        }
    }

    public static Bitmap rotateBitmapByDegree(Bitmap bitmap, int i) {
        Bitmap bitmap2;
        Bitmap bitmapCreateBitmap;
        Matrix matrix = new Matrix();
        matrix.postRotate(i);
        try {
            bitmap2 = bitmap;
            try {
                bitmapCreateBitmap = Bitmap.createBitmap(bitmap2, 0, 0, bitmap.getWidth(), bitmap.getHeight(), matrix, true);
            } catch (OutOfMemoryError unused) {
                bitmapCreateBitmap = null;
            }
        } catch (OutOfMemoryError unused2) {
            bitmap2 = bitmap;
        }
        if (bitmapCreateBitmap == null) {
            bitmapCreateBitmap = bitmap2;
        }
        if (bitmap2 != bitmapCreateBitmap) {
            bitmap2.recycle();
        }
        return bitmapCreateBitmap;
    }
}