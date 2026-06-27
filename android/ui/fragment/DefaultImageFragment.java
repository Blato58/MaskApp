package cn.com.heaton.shiningmask.ui.fragment;

import android.content.res.TypedArray;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ListAdapter;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.App;
import cn.com.heaton.shiningmask.base.BaseFragment;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.app.SoundManager;
import cn.com.heaton.shiningmask.databinding.FragmentDeuaultFragmentBinding;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.adapter.ImageListAdapter;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleManager;
import java.util.ArrayList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;

/* JADX INFO: loaded from: classes.dex */
public class DefaultImageFragment extends BaseFragment<FragmentDeuaultFragmentBinding> {
    private BleManager bleManager;
    private ArrayList<Integer> imageList;
    private ImageListAdapter imageListAdapter;

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initView(View view, Bundle bundle) {
    }

    public static DefaultImageFragment newInstance() {
        return new DefaultImageFragment();
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    public FragmentDeuaultFragmentBinding inflateBinding(LayoutInflater layoutInflater, ViewGroup viewGroup) {
        return FragmentDeuaultFragmentBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initData() {
        this.bleManager = ConnectActivity.getBleManager();
        initImageData();
        getBinding().lvImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.DefaultImageFragment.1
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                DefaultImageFragment.this.imageListAdapter.setSelectPosition(i);
                DefaultImageFragment.this.imageListAdapter.notifyDataSetChanged();
                SoundManager.getInstance().animSelect();
                DefaultImageFragment.this.sendImage(i);
            }
        });
    }

    private int[] getDefaultImageData(int i) {
        TypedArray typedArrayObtainTypedArray = getResources().obtainTypedArray(i);
        int length = typedArrayObtainTypedArray.length();
        int[] iArr = new int[typedArrayObtainTypedArray.length()];
        for (int i2 = 0; i2 < length; i2++) {
            iArr[i2] = typedArrayObtainTypedArray.getResourceId(i2, 0);
        }
        typedArrayObtainTypedArray.recycle();
        return iArr;
    }

    private void initImageData() {
        int[] defaultImageData;
        this.imageList = new ArrayList<>();
        if (ConnectActivity.isShowImageFlag()) {
            defaultImageData = getDefaultImageData(R.array.image_default_new);
        } else {
            defaultImageData = getDefaultImageData(R.array.image_default);
        }
        for (int i : defaultImageData) {
            this.imageList.add(Integer.valueOf(i));
        }
        this.imageListAdapter = new ImageListAdapter(getActivity(), this.imageList);
        getBinding().lvImage.setAdapter((ListAdapter) this.imageListAdapter);
    }

    public void sendImage(int i) {
        if (this.bleManager != null) {
            EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
            byte[] encryptData = Agreement.getEncryptData(Agreement.getImageCommand(i));
            List<BleDevice> deviceList = App.getAppData().getDeviceList();
            for (int i2 = 0; i2 < deviceList.size(); i2++) {
                deviceList.get(i2).writeCharacteristic(encryptData);
            }
        }
    }
}