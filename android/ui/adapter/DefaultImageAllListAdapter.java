package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.DefaultImage;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class DefaultImageAllListAdapter extends BaseAdapter {
    List<DefaultImage> brandsList;
    Context context;
    LayoutInflater mInflater;
    private int selectPosition = -1;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public DefaultImageAllListAdapter(Context context, List<DefaultImage> list) {
        this.context = context;
        this.brandsList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<DefaultImage> list) {
        this.brandsList = list;
    }

    public void setSelectPosition(int i) {
        this.selectPosition = i;
    }

    public int getSelectPosition() {
        return this.selectPosition;
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
            view = this.mInflater.inflate(R.layout.item_image_all_adapter, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.tv_select = (TextView) view.findViewById(R.id.tv_select);
            viewHolder.iv_crop_image = (ImageView) view.findViewById(R.id.iv_crop_image);
            viewHolder.ll_ledView = (LinearLayout) view.findViewById(R.id.ll_ledView);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        viewHolder.iv_crop_image.setImageResource(this.brandsList.get(i).getImgRes());
        int index = this.brandsList.get(i).getIndex();
        if (index > 0) {
            viewHolder.ll_ledView.setBackgroundResource(R.mipmap.item_image_deuault_bg_unselected);
            viewHolder.tv_select.setBackgroundResource(R.mipmap.all_item_slected);
            viewHolder.tv_select.setText(index + "");
            return view;
        }
        viewHolder.tv_select.setBackgroundResource(R.mipmap.all_item_unslected);
        viewHolder.ll_ledView.setBackgroundResource(R.mipmap.item_image_deuault_bg_selected);
        viewHolder.tv_select.setText("");
        return view;
    }

    public class ViewHolder {
        ImageView iv_crop_image;
        LinearLayout ll_ledView;
        TextView tv_select;

        public ViewHolder() {
        }
    }
}