package cn.com.heaton.shiningmask.ui.activity;

import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.net.Uri;
import android.provider.MediaStore;
import android.text.TextUtils;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.animation.AccelerateInterpolator;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseActivity;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.dao.bean.CropImage;
import cn.com.heaton.shiningmask.databinding.UcropActivityPhotoboxBinding;
import cn.com.heaton.shiningmask.model.data.DiyAgreement;
import cn.com.heaton.shiningmask.ui.utils.BitmapUtils;
import cn.com.heaton.shiningmask.ui.utils.ClickFilter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.ToastUtil;
import com.cdbwsoft.library.ble.BleDevice;
import com.yalantis.ucrop.R;
import com.yalantis.ucrop.UCrop;
import com.yalantis.ucrop.callback.BitmapCropCallback;
import com.yalantis.ucrop.model.AspectRatio;
import com.yalantis.ucrop.view.GestureCropImageView;
import com.yalantis.ucrop.view.OverlayView;
import com.yalantis.ucrop.view.TransformImageView;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class UCropActivity extends BaseActivity<UcropActivityPhotoboxBinding> implements View.OnClickListener {
    public static final Bitmap.CompressFormat DEFAULT_COMPRESS_FORMAT = Bitmap.CompressFormat.PNG;
    public static final int DEFAULT_COMPRESS_QUALITY = 90;
    private static final String TAG = "UCropActivity";
    List<Integer> bitmapDataList;
    int count;
    private CropImage cropImage;
    private Intent intent;
    private boolean isPreviewShow;
    private GestureCropImageView mGestureCropImageView;
    private OverlayView mOverlayView;
    private String url;
    private Bitmap.CompressFormat mCompressFormat = DEFAULT_COMPRESS_FORMAT;
    private int mCompressQuality = 90;
    private TransformImageView.TransformImageListener mImageListener = new TransformImageView.TransformImageListener() { // from class: cn.com.heaton.shiningmask.ui.activity.UCropActivity.1
        @Override // com.yalantis.ucrop.view.TransformImageView.TransformImageListener
        public void onRotate(float f) {
        }

        @Override // com.yalantis.ucrop.view.TransformImageView.TransformImageListener
        public void onScale(float f) {
        }

        @Override // com.yalantis.ucrop.view.TransformImageView.TransformImageListener
        public void onLoadComplete() {
            ((UcropActivityPhotoboxBinding) UCropActivity.this.getBinding()).ucrop.animate().alpha(1.0f).setDuration(300L).setInterpolator(new AccelerateInterpolator());
        }

        @Override // com.yalantis.ucrop.view.TransformImageView.TransformImageListener
        public void onLoadFailure(Exception exc) {
            UCropActivity.this.setResultError(exc);
        }
    };

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void bindListener() {
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    public UcropActivityPhotoboxBinding inflateBinding(LayoutInflater layoutInflater) {
        return UcropActivityPhotoboxBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initView() {
        getBinding().ivBack.setOnClickListener(this);
        getBinding().rlCropOk.setOnClickListener(this);
        getBinding().ivPreviewBack.setOnClickListener(this);
        getBinding().ivSave.setOnClickListener(this);
        this.intent = getIntent();
        initiateRootViews();
        setImageData(this.intent);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseActivity
    protected void initData() {
        this.mGestureCropImageView.setScaleEnabled(true);
        this.mGestureCropImageView.setRotateEnabled(false);
    }

    @Override // androidx.appcompat.app.AppCompatActivity, androidx.fragment.app.FragmentActivity, android.app.Activity
    protected void onStop() {
        super.onStop();
        GestureCropImageView gestureCropImageView = this.mGestureCropImageView;
        if (gestureCropImageView != null) {
            gestureCropImageView.cancelAllAnimations();
        }
    }

    private void setImageData(Intent intent) {
        Uri uri = (Uri) intent.getParcelableExtra(UCrop.EXTRA_INPUT_URI);
        Uri uri2 = (Uri) intent.getParcelableExtra(UCrop.EXTRA_OUTPUT_URI);
        LogUtil.d("inputUri:" + uri.getPath());
        LogUtil.d("outputUri:" + uri2);
        processOptions(intent);
        if (uri2 != null) {
            try {
                this.mGestureCropImageView.setImageUri(uri, uri2);
            } catch (Exception e) {
                setResultError(e);
            }
        }
    }

    private void processOptions(Intent intent) {
        String stringExtra = intent.getStringExtra(UCrop.Options.EXTRA_COMPRESSION_FORMAT_NAME);
        Bitmap.CompressFormat compressFormatValueOf = !TextUtils.isEmpty(stringExtra) ? Bitmap.CompressFormat.valueOf(stringExtra) : null;
        if (compressFormatValueOf == null) {
            compressFormatValueOf = DEFAULT_COMPRESS_FORMAT;
        }
        this.mCompressFormat = compressFormatValueOf;
        this.mCompressQuality = intent.getIntExtra(UCrop.Options.EXTRA_COMPRESSION_QUALITY, 90);
        intent.getIntArrayExtra(UCrop.Options.EXTRA_ALLOWED_GESTURES);
        this.mGestureCropImageView.setMaxBitmapSize(intent.getIntExtra(UCrop.Options.EXTRA_MAX_BITMAP_SIZE, 0));
        this.mGestureCropImageView.setMaxScaleMultiplier(intent.getFloatExtra(UCrop.Options.EXTRA_MAX_SCALE_MULTIPLIER, 10.0f));
        this.mGestureCropImageView.setImageToWrapCropBoundsAnimDuration(intent.getIntExtra(UCrop.Options.EXTRA_IMAGE_TO_CROP_BOUNDS_ANIM_DURATION, 500));
        this.mOverlayView.setFreestyleCropEnabled(intent.getBooleanExtra(UCrop.Options.EXTRA_FREE_STYLE_CROP, false));
        this.mOverlayView.setDimmedColor(intent.getIntExtra(UCrop.Options.EXTRA_DIMMED_LAYER_COLOR, getResources().getColor(R.color.ucrop_color_default_dimmed)));
        this.mOverlayView.setCircleDimmedLayer(intent.getBooleanExtra(UCrop.Options.EXTRA_CIRCLE_DIMMED_LAYER, false));
        this.mOverlayView.setShowCropFrame(intent.getBooleanExtra(UCrop.Options.EXTRA_SHOW_CROP_FRAME, true));
        this.mOverlayView.setCropFrameColor(intent.getIntExtra(UCrop.Options.EXTRA_CROP_FRAME_COLOR, getResources().getColor(R.color.ucrop_color_default_crop_frame)));
        this.mOverlayView.setCropFrameStrokeWidth(intent.getIntExtra(UCrop.Options.EXTRA_CROP_FRAME_STROKE_WIDTH, getResources().getDimensionPixelSize(R.dimen.ucrop_default_crop_frame_stoke_width)));
        this.mOverlayView.setShowCropGrid(intent.getBooleanExtra(UCrop.Options.EXTRA_SHOW_CROP_GRID, true));
        this.mOverlayView.setCropGridRowCount(intent.getIntExtra(UCrop.Options.EXTRA_CROP_GRID_ROW_COUNT, 2));
        this.mOverlayView.setCropGridColumnCount(intent.getIntExtra(UCrop.Options.EXTRA_CROP_GRID_COLUMN_COUNT, 2));
        this.mOverlayView.setCropGridColor(intent.getIntExtra(UCrop.Options.EXTRA_CROP_GRID_COLOR, getResources().getColor(R.color.ucrop_color_default_crop_grid)));
        this.mOverlayView.setCropGridStrokeWidth(intent.getIntExtra(UCrop.Options.EXTRA_CROP_GRID_STROKE_WIDTH, getResources().getDimensionPixelSize(R.dimen.ucrop_default_crop_grid_stoke_width)));
        float floatExtra = intent.getFloatExtra(UCrop.EXTRA_ASPECT_RATIO_X, 0.0f);
        float floatExtra2 = intent.getFloatExtra(UCrop.EXTRA_ASPECT_RATIO_Y, 0.0f);
        int intExtra = intent.getIntExtra(UCrop.Options.EXTRA_ASPECT_RATIO_SELECTED_BY_DEFAULT, 0);
        ArrayList parcelableArrayListExtra = intent.getParcelableArrayListExtra(UCrop.Options.EXTRA_ASPECT_RATIO_OPTIONS);
        if (floatExtra > 0.0f && floatExtra2 > 0.0f) {
            this.mGestureCropImageView.setTargetAspectRatio(floatExtra / floatExtra2);
        } else if (parcelableArrayListExtra != null && intExtra < parcelableArrayListExtra.size()) {
            this.mGestureCropImageView.setTargetAspectRatio(((AspectRatio) parcelableArrayListExtra.get(intExtra)).getAspectRatioX() / ((AspectRatio) parcelableArrayListExtra.get(intExtra)).getAspectRatioY());
        } else {
            this.mGestureCropImageView.setTargetAspectRatio(0.0f);
        }
        int intExtra2 = intent.getIntExtra(UCrop.EXTRA_MAX_SIZE_X, 0);
        int intExtra3 = intent.getIntExtra(UCrop.EXTRA_MAX_SIZE_Y, 0);
        if (intExtra2 <= 0 || intExtra3 <= 0) {
            return;
        }
        this.mGestureCropImageView.setMaxResultImageSizeX(intExtra2);
        this.mGestureCropImageView.setMaxResultImageSizeY(intExtra3);
    }

    private void initiateRootViews() {
        this.mGestureCropImageView = getBinding().ucrop.getCropImageView();
        this.mOverlayView = getBinding().ucrop.getOverlayView();
        this.mGestureCropImageView.setTransformImageListener(this.mImageListener);
    }

    protected void cropAndSaveImage() {
        Log.e(TAG, "裁剪图片");
        this.mGestureCropImageView.cropAndSaveImage(this.mCompressFormat, this.mCompressQuality, new BitmapCropCallback() { // from class: cn.com.heaton.shiningmask.ui.activity.UCropActivity.2
            @Override // com.yalantis.ucrop.callback.BitmapCropCallback
            public void onCropFailure(Throwable th) {
            }

            @Override // com.yalantis.ucrop.callback.BitmapCropCallback
            public void onBitmapCropped(Uri uri, int i, int i2, int i3, int i4) {
                UCropActivity uCropActivity = UCropActivity.this;
                uCropActivity.setResultUri(uri, uCropActivity.mGestureCropImageView.getTargetAspectRatio(), i, i2, i3, i4);
                UCropActivity.this.showPreview();
                try {
                    Bitmap bitmap = MediaStore.Images.Media.getBitmap(UCropActivity.this.getContentResolver(), uri);
                    UCropActivity.this.bitmapDataList = BitmapUtils.getBitmapData(bitmap);
                    LogUtil.d("保存后的颜色：" + UCropActivity.this.bitmapDataList.get(0) + " length:" + UCropActivity.this.bitmapDataList.size());
                    Bitmap bitmapBitmapCrop = BitmapUtils.bitmapCrop(bitmap);
                    ((UcropActivityPhotoboxBinding) UCropActivity.this.getBinding()).ivPreview.setImageBitmap(bitmapBitmapCrop);
                    UCropActivity uCropActivity2 = UCropActivity.this;
                    uCropActivity2.url = BitmapUtils.saveToLocalPNG(uCropActivity2.mActivity, bitmapBitmapCrop, "diyimage");
                    LogUtil.d("保存后的图片路径：" + UCropActivity.this.url);
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                } catch (IOException e2) {
                    e2.printStackTrace();
                }
            }
        });
    }

    protected void setResultUri(Uri uri, float f, int i, int i2, int i3, int i4) {
        setResult(-1, new Intent().putExtra(UCrop.EXTRA_OUTPUT_URI, uri).putExtra(UCrop.EXTRA_OUTPUT_CROP_ASPECT_RATIO, f).putExtra(UCrop.EXTRA_OUTPUT_IMAGE_WIDTH, i3).putExtra(UCrop.EXTRA_OUTPUT_IMAGE_HEIGHT, i4).putExtra(UCrop.EXTRA_OUTPUT_OFFSET_X, i).putExtra(UCrop.EXTRA_OUTPUT_OFFSET_Y, i2));
    }

    protected void setResultError(Throwable th) {
        setResult(96, new Intent().putExtra(UCrop.EXTRA_ERROR, th));
    }

    @Override // android.view.View.OnClickListener
    public void onClick(View view) {
        if (ClickFilter.filter()) {
            return;
        }
        int id = view.getId();
        if (id == cn.com.heaton.shiningmask.R.id.iv_back) {
            finish();
            return;
        }
        if (id == cn.com.heaton.shiningmask.R.id.rl_crop_ok) {
            cropAndSaveImage();
            return;
        }
        if (id == cn.com.heaton.shiningmask.R.id.iv_preview_back) {
            hidePreview();
            return;
        }
        if (id != cn.com.heaton.shiningmask.R.id.iv_save || TextUtils.isEmpty(this.url)) {
            return;
        }
        LogUtil.d("保存的图片url:" + this.url);
        byte[] sendImageData = getSendImageData(this.bitmapDataList);
        if (sendImageData == null) {
            return;
        }
        List<CropImage> listLoadAll = App.getDaoSession().getCropImageDao().loadAll();
        ArrayList arrayList = new ArrayList();
        StringBuilder sb = new StringBuilder();
        if (listLoadAll != null) {
            for (int i = 0; i < listLoadAll.size(); i++) {
                arrayList.add(Integer.valueOf(listLoadAll.get(i).getImageIndex()));
                sb.append(listLoadAll.get(i).getImageIndex() + ",");
            }
        }
        LogUtil.d("sbString:" + sb.toString());
        int i2 = 1;
        int i3 = 1;
        while (true) {
            if (i3 > 20) {
                break;
            }
            if (!sb.toString().contains(i3 + "")) {
                i2 = i3;
                break;
            }
            i3++;
        }
        LogUtil.d("index::::" + i2);
        int iCurrentTimeMillis = (int) (System.currentTimeMillis() / 1000);
        CropImage cropImage = new CropImage(this.url, sendImageData, i2);
        this.cropImage = cropImage;
        cropImage.setTimeInt(iCurrentTimeMillis);
        sendImageData(this.cropImage);
    }

    @Override // androidx.activity.ComponentActivity, android.app.Activity
    public void onBackPressed() {
        if (this.isPreviewShow) {
            hidePreview();
        } else {
            super.onBackPressed();
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void toActivity() {
        LogUtil.d("保存图片数据到数据库：" + this.cropImage.getImageIndex());
        App.getDaoSession().getCropImageDao().save(this.cropImage);
        if (this.intent.getIntExtra("flag", 0) == 1) {
            EventBus.getDefault().post(C.MAIN_EVENT.UPDATE_DIY_LIST);
            LogUtil.d("从图片列表界面过来");
        } else {
            Intent intent = new Intent(this.mActivity, (Class<?>) ImageActivity.class);
            intent.putExtra("flag", 1);
            startActivity(intent);
        }
        EventBus.getDefault().post(C.MAIN_EVENT.FINISH_CAMERAACTIVITY);
        finish();
    }

    private void hidePreview() {
        setImageData(this.intent);
        this.isPreviewShow = false;
        getBinding().rlImageCrop.setVisibility(0);
        getBinding().rlImagePreview.setVisibility(8);
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void showPreview() {
        this.isPreviewShow = true;
        getBinding().rlImageCrop.setVisibility(8);
        getBinding().rlImagePreview.setVisibility(0);
    }

    private void sendImageData(CropImage cropImage) {
        this.count = 0;
        final List<BleDevice> deviceList = App.getAppData().getDeviceList();
        EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        DiyAgreement.DiyAgreementListener diyAgreementListener = new DiyAgreement.DiyAgreementListener() { // from class: cn.com.heaton.shiningmask.ui.activity.UCropActivity.3
            @Override // cn.com.heaton.shiningmask.model.data.DiyAgreement.DiyAgreementListener
            public void onFinishSend(BleDevice bleDevice) {
                UCropActivity.this.count++;
                LogUtil.d("=====diy数据发送完成");
                if (deviceList.size() == UCropActivity.this.count) {
                    UCropActivity.this.dismissProgressDialog();
                    ToastUtil.showToast(UCropActivity.this.getString(cn.com.heaton.shiningmask.R.string.send_successfully));
                    UCropActivity.this.toActivity();
                }
            }
        };
        if (deviceList != null && !deviceList.isEmpty()) {
            DiyAgreement diyAgreement = DiyAgreement.getInstance();
            showProgressDialog(this.mContext, getString(cn.com.heaton.shiningmask.R.string.send));
            for (int i = 0; i < deviceList.size(); i++) {
                diyAgreement.sendDiy(deviceList.get(i), cropImage, diyAgreementListener);
            }
            return;
        }
        toActivity();
    }

    private byte[] getSendImageData(List<Integer> list) {
        if (list == null) {
            return null;
        }
        byte[] bArr = new byte[list.size() * 3];
        ArrayList arrayList = new ArrayList();
        for (int i = 0; i < list.size(); i++) {
            arrayList.add(Byte.valueOf((byte) Color.red(list.get(i).intValue())));
            arrayList.add(Byte.valueOf((byte) Color.green(list.get(i).intValue())));
            arrayList.add(Byte.valueOf((byte) Color.blue(list.get(i).intValue())));
        }
        for (int i2 = 0; i2 < arrayList.size(); i2++) {
            bArr[i2] = ((Byte) arrayList.get(i2)).byteValue();
        }
        return bArr;
    }
}