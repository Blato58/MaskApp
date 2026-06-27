package cn.com.heaton.shiningmask.ui.fragment;

import android.os.Bundle;
import android.util.Log;
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
import cn.com.heaton.shiningmask.databinding.FragmentAnimBinding;
import cn.com.heaton.shiningmask.model.data.Agreement;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.adapter.AnimListAdapter;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.cdbwsoft.library.ble.BleDevice;
import com.cdbwsoft.library.ble.BleManager;
import java.util.ArrayList;
import java.util.List;
import org.greenrobot.eventbus.EventBus;
import org.greenrobot.eventbus.Subscribe;

/* JADX INFO: loaded from: classes.dex */
public class AnimFragment extends BaseFragment<FragmentAnimBinding> {
    private static boolean isClick = false;
    private BleManager bleManager;
    private int curPosition = -1;
    private AnimListAdapter imageListAdapter;
    private ArrayList<Integer> namesList;

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initView(View view, Bundle bundle) {
    }

    public static AnimFragment newInstance(boolean z) {
        return new AnimFragment();
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    public FragmentAnimBinding inflateBinding(LayoutInflater layoutInflater, ViewGroup viewGroup) {
        return FragmentAnimBinding.inflate(layoutInflater);
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment
    protected void initData() {
        EventBus.getDefault().register(this);
        this.bleManager = ConnectActivity.getBleManager();
        initDatas1664();
        getBinding().lvImage.setOnItemClickListener(new AdapterView.OnItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.fragment.AnimFragment.1
            @Override // android.widget.AdapterView.OnItemClickListener
            public void onItemClick(AdapterView<?> adapterView, View view, int i, long j) {
                SoundManager.getInstance().animSelect();
                LogUtil.d("position:" + i);
                LogUtil.d("curPosition:" + AnimFragment.this.curPosition);
                AnimFragment.this.imageListAdapter.setSelectPosition(i);
                AnimFragment animFragment = AnimFragment.this;
                animFragment.updateItem(animFragment.curPosition);
                AnimFragment.this.curPosition = i;
                AnimFragment animFragment2 = AnimFragment.this;
                animFragment2.updateItem(animFragment2.curPosition);
                AnimFragment.this.sendAmin(i);
            }
        });
    }

    private void initDatas1664() {
        ArrayList<Integer> arrayList = new ArrayList<>();
        this.namesList = arrayList;
        arrayList.add(Integer.valueOf(R.array.anim0));
        this.namesList.add(Integer.valueOf(R.array.anim1));
        this.namesList.add(Integer.valueOf(R.array.anim2));
        this.namesList.add(Integer.valueOf(R.array.anim3));
        this.namesList.add(Integer.valueOf(R.array.anim5));
        this.namesList.add(Integer.valueOf(R.array.anim6));
        this.namesList.add(Integer.valueOf(R.array.anim7));
        this.namesList.add(Integer.valueOf(R.array.anim8));
        this.namesList.add(Integer.valueOf(R.array.anim9));
        this.namesList.add(Integer.valueOf(R.array.anim10));
        this.namesList.add(Integer.valueOf(R.array.anim11));
        this.namesList.add(Integer.valueOf(R.array.anim12));
        this.namesList.add(Integer.valueOf(R.array.anim13));
        this.namesList.add(Integer.valueOf(R.array.anim14));
        this.namesList.add(Integer.valueOf(R.array.anim15));
        this.namesList.add(Integer.valueOf(R.array.anim16));
        this.namesList.add(Integer.valueOf(R.array.anim17));
        this.namesList.add(Integer.valueOf(R.array.anim18));
        this.namesList.add(Integer.valueOf(R.array.anim19));
        this.namesList.add(Integer.valueOf(R.array.anim20));
        this.namesList.add(Integer.valueOf(R.array.anim21));
        this.namesList.add(Integer.valueOf(R.array.anim22));
        this.namesList.add(Integer.valueOf(R.array.anim23));
        this.namesList.add(Integer.valueOf(R.array.anim24));
        this.namesList.add(Integer.valueOf(R.array.anim25));
        this.namesList.add(Integer.valueOf(R.array.anim26));
        this.namesList.add(Integer.valueOf(R.array.anim27));
        this.namesList.add(Integer.valueOf(R.array.anim28));
        this.namesList.add(Integer.valueOf(R.array.anim29));
        this.namesList.add(Integer.valueOf(R.array.anim30));
        this.namesList.add(Integer.valueOf(R.array.anim31));
        this.namesList.add(Integer.valueOf(R.array.anim32));
        this.namesList.add(Integer.valueOf(R.array.anim33));
        this.namesList.add(Integer.valueOf(R.array.anim34));
        this.namesList.add(Integer.valueOf(R.array.anim35));
        this.namesList.add(Integer.valueOf(R.array.anim36));
        this.namesList.add(Integer.valueOf(R.array.anim37));
        this.namesList.add(Integer.valueOf(R.array.anim38));
        this.namesList.add(Integer.valueOf(R.array.anim39));
        this.namesList.add(Integer.valueOf(R.array.anim40));
        this.namesList.add(Integer.valueOf(R.array.anim41));
        this.namesList.add(Integer.valueOf(R.array.anim42));
        this.namesList.add(Integer.valueOf(R.array.anim43));
        this.namesList.add(Integer.valueOf(R.array.anim44));
        this.namesList.add(Integer.valueOf(R.array.anim45));
        this.imageListAdapter = new AnimListAdapter(getActivity(), this.namesList);
        getBinding().lvImage.setAdapter((ListAdapter) this.imageListAdapter);
        this.imageListAdapter.notifyDataSetChanged();
    }

    @Override // cn.com.heaton.shiningmask.base.BaseFragment, androidx.fragment.app.Fragment
    public void onDestroy() {
        super.onDestroy();
        EventBus.getDefault().unregister(this);
    }

    public void sendAmin(int i) {
        if (this.bleManager == null) {
            return;
        }
        if (i >= 4) {
            i++;
            LogUtil.d("发送动画命令>>>：" + i);
        }
        EventBus.getDefault().post(C.MAIN_EVENT.STOP_RHY);
        int i2 = 0;
        if (i >= 0) {
            byte[] encryptData = Agreement.getEncryptData(Agreement.getAnimCommand(i));
            List<BleDevice> deviceList = App.getAppData().getDeviceList();
            while (i2 < deviceList.size()) {
                deviceList.get(i2).writeCharacteristic(encryptData);
                i2++;
            }
            return;
        }
        byte[] encryptData2 = Agreement.getEncryptData(Agreement.getAnimLoopCommand());
        List<BleDevice> deviceList2 = App.getAppData().getDeviceList();
        while (i2 < deviceList2.size()) {
            deviceList2.get(i2).writeCharacteristic(encryptData2);
            i2++;
        }
    }

    @Subscribe
    public void updateAnimList(String str) {
        if (C.MAIN_EVENT.UPDATE_ANIM.equals(str)) {
            this.curPosition = -1;
            this.imageListAdapter.setSelectPosition(-1);
            this.imageListAdapter.notifyDataSetChanged();
            sendAmin(-1);
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void updateItem(int i) {
        int firstVisiblePosition = getBinding().lvImage.getFirstVisiblePosition();
        Log.e("BaseFragment", "updateItem: " + i);
        int lastVisiblePosition = getBinding().lvImage.getLastVisiblePosition();
        if (i < firstVisiblePosition || i > lastVisiblePosition) {
            return;
        }
        this.imageListAdapter.getView(i, getBinding().lvImage.getChildAt(i - firstVisiblePosition), getBinding().lvImage);
    }
}