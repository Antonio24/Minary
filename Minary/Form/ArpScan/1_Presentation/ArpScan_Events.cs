﻿namespace Minary.Form.ArpScan.Presentation
{
  using Minary.Common;
  using Minary.DataTypes.ArpScan;
  using Minary.DataTypes.Enum;
  using Minary.Domain.ArpScan;
  using Minary.Form.ArpScan.DataTypes;
  using Minary.LogConsole.Main;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Windows.Forms;


  public partial class ArpScan : IObserverArpRequest
  {


    #region MEMBERS

    private Action onScanDoneCallback;

    #endregion

    
    #region PUBLIC

    public delegate void HideArpScanWindowDelegate();
    public void HideArpScanWindow()
    {
      if (this.InvokeRequired == true)
      {
        this.BeginInvoke(new HideArpScanWindowDelegate(this.HideArpScanWindow), new object[] { });
        return;
      }

      // Reset progress bar value
      this.pb_ArpScan.Value = 0;

      // Stop running ARP scan
      this.bgw_ArpScanSender.CancelAsync();

      // Send targetSystem list to modules
      //this.minaryMain.PassNewTargetListToPlugins();

      // Hide form insead of closing.
      this.Hide();
      this.Owner.Activate();
      this.Owner.Show();
    }

    #endregion


    #region EVENTS

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
    {
      // Ignore clicks that are not on button cells.
      if (e.RowIndex < 0)
      {
        return;
      }

      var ipAddress = this.dgv_Targets.Rows[e.RowIndex].Cells[0].Value.ToString();
      var macAddress = this.dgv_Targets.Rows[e.RowIndex].Cells[1].Value.ToString();
      var vendor = this.dgv_Targets.Rows[e.RowIndex].Cells[2].Value.ToString();

      // (De)Activate targetSystem system
      if (e.ColumnIndex != 3)
      {
        return;
      }

      for (var i = 0; i < this.TargetList.Count; i++)
      {
        if (this.TargetList[i].MacAddress == macAddress && 
            this.TargetList[i].IpAddress == ipAddress)
        {
          this.TargetList[i].Attack = this.TargetList[i].Attack ? false : true;
          break;
        }
      }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Dgv_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      if (this.dgv_Targets.IsCurrentCellDirty)
      {
        this.dgv_Targets.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="e"></param>
    private void Dgv_CellValueChanged(object obj, DataGridViewCellEventArgs e)
    {
      // compare to checkBox column index
      if (e.ColumnIndex != 0)
      {
        return;
      }

      DataGridViewCheckBoxCell check = this.dgv_Targets[0, e.RowIndex] as DataGridViewCheckBoxCell;
      if (Convert.ToBoolean(check.Value) == true)
      {
        // If tick is added!
        // ...
      }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BT_Close_Click(object sender, EventArgs e)
    {
      this.HideArpScanWindow();
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BT_Scan_Click(object sender, EventArgs e)
    {
      this.StartArpScan();
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ArpScan_FormClosing(object sender, FormClosingEventArgs e)
    {
      this.HideArpScanWindow();
      e.Cancel = true;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RB_Subnet_CheckedChanged(object sender, EventArgs e)
    {
      this.tb_Netrange1.ReadOnly = true;
      this.tb_Netrange2.ReadOnly = true;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RB_Netrange_CheckedChanged(object sender, EventArgs e)
    {
      this.tb_Netrange1.ReadOnly = false;
      this.tb_Netrange2.ReadOnly = false;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Tb_Netrange2_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Enter)
      {
        return;
      }

      this.StartArpScan();
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TB_Netrange1_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Enter)
      {
        return;
      }

      this.StartArpScan();
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.TargetList == null ||
          this.TargetList.Count <= 0)
      {
        return;
      }

      for (int i = 0; i < this.dgv_Targets.Rows.Count; i++)
      {
        try
        {
          this.dgv_Targets.Rows[i].Cells["Attack"].Value = true;
        }
        catch
        {
        }
      }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Dgv_Targets_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
      {
        return;
      }

      try
      {
        DataGridView.HitTestInfo hti = this.dgv_Targets.HitTest(e.X, e.Y);
        if (hti.RowIndex >= 0)
        {
          this.cms_ManageTargets.Show(this.dgv_Targets, e.Location);
        }
      }
      catch
      {
      }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UnselectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (this.TargetList == null || 
          this.TargetList.Count <= 0)
      {
        return;
      }

      for (var i = 0; i < this.dgv_Targets.Rows.Count; i++)
      {
        Utils.TryExecute2(() => { this.dgv_Targets.Rows[i].Cells["Attack"].Value = false; });
      }
    }


    /// <summary>
    /// Close Sessions GUI on Escape.
    /// </summary>
    /// <param name="keyData"></param>
    /// <returns></returns>
    protected override bool ProcessDialogKey(Keys keyData)
    {
      if (keyData == Keys.Escape)
      {
        this.HideArpScanWindow();
        return true;
      }

      return base.ProcessDialogKey(keyData);
    }
    

    /// <summary>
    ///
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Dgv_Targets_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        DataGridView.HitTestInfo hti = this.dgv_Targets.HitTest(e.X, e.Y);

        if (hti.RowIndex >= 0)
        {
          this.dgv_Targets.ClearSelection();
          this.dgv_Targets.Rows[hti.RowIndex].Selected = true;
          this.dgv_Targets.CurrentCell = this.dgv_Targets.Rows[hti.RowIndex].Cells[0];
        }
      }
      catch (Exception ex)
      {
        LogCons.Inst.Write(LogLevel.Error, $"ArpScan: {ex.Message}");
        this.dgv_Targets.ClearSelection();
      }
    }

    #endregion


    #region EVENTS: BGW_ArpScanSender

    private void BGW_ArpScanSender_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        LogCons.Inst.Write(LogLevel.Error, "BGW_ArpScanSender(): Completed with error");
        this.pb_ArpScan.Value = 0;
      }
      else if (e.Cancelled == true)
      {
        LogCons.Inst.Write(LogLevel.Info, "BGW_ArpScanSender(): Completed by cancellation");
        this.pb_ArpScan.Value = 0;
      }
      else
      {
        LogCons.Inst.Write(LogLevel.Info, $"BGW_ArpScanSender(): Completed successfully. value={this.pb_ArpScan.Value}, maximum={this.pb_ArpScan.Maximum}");
        this.pb_ArpScan.PerformStep();
      }

      this.SetArpScanGuiOnStopped();

      // Call caller callback function after scan has completed.
      if (this.onScanDoneCallback != null)
      {
        this.onScanDoneCallback();
      }
    }


    private void BGW_ArpScanSender_DoWork(object sender, DoWorkEventArgs e)
    {
      ArpScanConfig arpScanConfig = null;
      ArpScanner arpScanner = null;

      try
      {
        arpScanConfig = this.GetArpScanConfig();
        arpScanner = new ArpScanner(arpScanConfig);
      }
      catch (Exception ex)
      {
        LogCons.Inst.Write(LogLevel.Error, $"BGW_ArpScanSender(EXCEPTION): {ex.Message}\r\n{ex.StackTrace}");
        this.SetArpScanGuiOnStopped();
      }

      try
      {
        arpScanner.AddObserver(this);
        arpScanner.StartScanning();
      }
      catch (Exception ex)
      {
        LogCons.Inst.Write(LogLevel.Error, $"BGW_ArpScanSender(EXCEPTION2): {ex.Message}\r\n{ex.StackTrace}");
        this.SetArpScanGuiOnStopped();
        return;
      }
    }

    #endregion


    #region EVENTS: BGW_ArpScanListener

    private void BGW_ArpScanListener_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        LogCons.Inst.Write(LogLevel.Error, "BGW_ArpScanListener(): Completed with error");
      }
      else if (e.Cancelled == true)
      {
        LogCons.Inst.Write(LogLevel.Info, "BGW_ArpScanListener(): Completed by cancellation");
      }
      else
      {
        LogCons.Inst.Write(LogLevel.Info, "BGW_ArpScanListener(): Completed successfully");
      }
    }


    private void BGW_ArpScanListener_DoWork(object sender, DoWorkEventArgs e)
    {
      ArpScanConfig arpScanConfig = null;
      ReplyListener replyListener = null;

      try
      {
        arpScanConfig = this.GetArpScanConfig();
        replyListener = new ReplyListener(arpScanConfig);
      }
      catch (Exception ex)
      {
        LogCons.Inst.Write(LogLevel.Error, $"BGW_ArpScanListener(EXCEPTION1): {ex.Message}");
        return;
      }

      try
      {
        replyListener.AddObserver(this);
        replyListener.StartReceivingArpPackets();
      }
      catch (Exception ex)
      {
        LogCons.Inst.Write(LogLevel.Error, $"BGW_ArpScanListener(EXCEPTION2): {ex.Message}");
        return;
      }

      LogCons.Inst.Write(LogLevel.Info, "BGW_ArpScanListener(): Background worker is started");
    }

    #endregion


    #region PRIVATE

    public void StartArpScan(Action onScanDoneCallback = null)
    {
      // Assign callers "done event". Important! For DSL calls!
      this.onScanDoneCallback = onScanDoneCallback;

      // Initiate ARP scan cancellation
      if (this.bgw_ArpScanSender.IsBusy == true &&
          this.bgw_ArpScanSender.CancellationPending == false)
      {
        LogCons.Inst.Write(LogLevel.Info, "ArpScan: Cancel running ARP scan");
        this.bgw_ArpScanSender.CancelAsync();
      }
      else if (this.bgw_ArpScanSender.IsBusy == true &&
               this.bgw_ArpScanSender.CancellationPending == true)
      {
        LogCons.Inst.Write(LogLevel.Info, "ArpScan: Cancellation running");
      }
      else
      {
        LogCons.Inst.Write(LogLevel.Info, "ArpScan: ArpScan started");

        // Set Progress bar structure
        this.pb_ArpScan.Maximum = 100;
        this.pb_ArpScan.Minimum = 0;
        this.pb_ArpScan.Value = 0;
        this.pb_ArpScan.Step = 10;

        // Initiate start
        this.TargetList.Clear();
        this.SetArpScanGuiOnStarted();
        this.bgw_ArpScanSender.RunWorkerAsync();
      }
    }


    private ArpScanConfig GetArpScanConfig()
    {
      var startIp = string.Empty;
      var stopIp = string.Empty;

      if (this.rb_Netrange.Checked == true)
      {
        startIp = this.tb_Netrange1.Text;
        stopIp = this.tb_Netrange2.Text;
      }
      else
      {
        startIp = this.tb_Subnet1.Text;
        stopIp = this.tb_Subnet2.Text;
      }

      ArpScanConfig arpScanConfig = new ArpScanConfig()
      {
        InterfaceId = this.interfaceId,
        GatewayIp = this.gatewayIp,
        LocalIp = this.localIp,
        LocalMac = this.localMac?.Replace('-', ':'),
        NetworkStartIp = startIp,
        NetworkStopIp = stopIp,
        MaxNumberSystemsToScan = -1,
        ObserverClass = this,
        Communicator = this.communicator
      };

      return arpScanConfig;
    }

    #endregion

  }
}