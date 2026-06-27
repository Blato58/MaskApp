package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.LinearLayout;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.FrameAnimationUtils;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class AnimListAdapter extends BaseAdapter {
    List<Integer> brandsList;
    Context context;
    private int goneIndex;
    LayoutInflater mInflater;
    private int selectPosition = -1;
    Map<Integer, FrameAnimationUtils.FramesSequenceAnimation> map = new HashMap();

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public AnimListAdapter(Context context, List<Integer> list) {
        this.context = context;
        this.brandsList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<Integer> list) {
        this.brandsList = list;
    }

    public void setSelectPosition(int i) {
        this.selectPosition = i;
    }

    public int getSelectPosition() {
        return this.selectPosition;
    }

    public void setGoneIndex(int i) {
        this.goneIndex = i;
    }

    @Override // android.widget.Adapter
    public int getCount() {
        return this.brandsList.size();
    }

    @Override // android.widget.Adapter
    public Object getItem(int i) {
        return Integer.valueOf(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        ViewHolder viewHolder;
        if (view == null) {
            view = this.mInflater.inflate(R.layout.item_image_adapter, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.iv_item = (ImageView) view.findViewById(R.id.item_ledview);
            viewHolder.ll_ledView1 = (LinearLayout) view.findViewById(R.id.ll_ledView1);
            viewHolder.animation = new FrameAnimationUtils(this.context).createAnim(viewHolder.iv_item, 100);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        if (this.selectPosition == i) {
            viewHolder.ll_ledView1.setBackgroundResource(R.mipmap.item_image_deuault_bg_unselected);
            viewHolder.iv_item.setAlpha(1.0f);
        } else {
            viewHolder.ll_ledView1.setBackgroundResource(R.mipmap.item_image_deuault_bg_selected);
            viewHolder.iv_item.setAlpha(0.65f);
        }
        viewHolder.animation.setResId(this.brandsList.get(i).intValue());
        viewHolder.animation.start();
        return view;
    }

    public class ViewHolder {
        FrameAnimationUtils.FramesSequenceAnimation animation;
        ImageView iv_item;
        LinearLayout ll_ledView1;

        public ViewHolder() {
        }
    }
}