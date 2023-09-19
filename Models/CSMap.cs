using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapBanSimulator.Models;

public class CSMap
{
    public string? Name { get; set; }

    private bool _isBanned;
    public bool IsBanned
    {
        get => _isBanned;
        set
        {
            _isBanned = value;
            OnPropertyChanged(nameof(IsBanned));
        }
    }

    private int? _rank;
    public int? Rank
    {
        get => _rank;
        set
        {
            _rank = value;
            OnPropertyChanged(nameof(Rank));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
