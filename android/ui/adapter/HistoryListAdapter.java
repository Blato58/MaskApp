package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.HistoryData;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.widget.LedTextView;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class HistoryListAdapter extends BaseAdapter {
    List<HistoryData> brandsList;
    Context context;
    LayoutInflater mInflater;
    private int selectPosition = -1;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public HistoryListAdapter(Context context, List<HistoryData> list) {
        this.context = context;
        this.brandsList = list;
        this.mInflater = (LayoutInflater) context.getSystemService("layout_inflater");
    }

    public void setList(List<HistoryData> list) {
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
        List<HistoryData> list = this.brandsList;
        if (list == null) {
            return 0;
        }
        if (list.size() > 10) {
            return 10;
        }
        return this.brandsList.size();
    }

    @Override // android.widget.Adapter
    public Object getItem(int i) {
        return Integer.valueOf(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        LogUtil.d("convertView:" + i);
        View viewInflate = this.mInflater.inflate(R.layout.item_history, viewGroup, false);
        ViewHolder viewHolder = new ViewHolder();
        viewHolder.iv_item = (LedTextView) viewInflate.findViewById(R.id.item_ledview);
        viewHolder.iv_item.setLayerType(1, null);
        viewHolder.iv_item.setPointMargin(0);
        viewHolder.iv_item.removeAllViews();
        viewHolder.iv_item.init(128, 16);
        viewHolder.iv_item.setHistoryData(this.brandsList.get(i).getData(), this.brandsList.get(i).getColorArray());
        return viewInflate;
    }

    public class ViewHolder {
        LedTextView iv_item;

        public ViewHolder() {
        }
    }
}