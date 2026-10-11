using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Globalization;

namespace CalculadoraNETMAUI
{
    public partial class MainPage : ContentPage
    {

        private static readonly CultureInfo _cultura = new CultureInfo("es-ES");
        private const int MaximoDigitos = 12;
        private const int MaximoCaracteresPrincipal = 26;
        private const int MaximoCaracteresSuperior = 34;

        private double _memoria = 0;
        private double _acumulado = 0;
        private string _operadorPendiente = string.Empty;
        private string _expresionPrevia = string.Empty;
        private string _numeroActual = "0";
        private string _operacionTerminada = string.Empty;
        private string _textoSubtotal = string.Empty;
        private bool _resultadoMostrado = false;
        private bool _sustituirAlEscribir = false;
        private bool _hayError = false;

        public MainPage()
        {
            InitializeComponent();
            Loaded += OnPaginaCargada;
        }

        private void OnPaginaCargada(object? sender, EventArgs e)
        {
        #if WINDOWS
                ConfigurarTecladoWindows();
        #endif
        }

        private void OnNumeroClicked(object? sender, EventArgs e)
        {
            Button? boton = sender as Button;
            if (boton == null) return;

            PulsarNumero(boton.Text);
        }

        private void OnComaClicked(object? sender, EventArgs e)
        {
            PulsarComa();
        }

        private void OnOperadorClicked(object? sender, EventArgs e)
        {
            Button? boton = sender as Button;
            if (boton == null) return;

            PulsarOperador(boton.Text);
        }

        private void OnIgualClicked(object? sender, EventArgs e)
        {
            PulsarIgual();
        }

        private void OnBorrarClicked(object? sender, EventArgs e)
        {
            LimpiarOperacion();
        }

        private void PulsarNumero(string digito)
        {
            if (_hayError || _resultadoMostrado) LimpiarOperacion();

            if (_numeroActual == "0" || _numeroActual == string.Empty || _sustituirAlEscribir)
            {
                _numeroActual = digito;
                _sustituirAlEscribir = false;
            }
            else if (_numeroActual.Length < MaximoDigitos)
            {
                _numeroActual += digito;
            }

            ActualizarPantalla();
        }

        private void PulsarComa()
        {
            if (_hayError || _resultadoMostrado) LimpiarOperacion();

            if (_numeroActual == string.Empty || _sustituirAlEscribir)
            {
                _numeroActual = "0,";
                _sustituirAlEscribir = false;
            }
            else if (!_numeroActual.Contains(',') && _numeroActual.Length < MaximoDigitos)
            {
                _numeroActual += ",";
            }

            ActualizarPantalla();
        }

        private void PulsarOperador(string operador)
        {
            if (_hayError) return;

            if (_resultadoMostrado)
            {
                _resultadoMostrado = false;
                _expresionPrevia = string.Empty;
                _textoSubtotal = string.Empty;
                _operadorPendiente = string.Empty;

            }

            if (_numeroActual == string.Empty)
            {
                string sinOperador = _expresionPrevia.TrimEnd();
                sinOperador = sinOperador.Substring(0, sinOperador.Length - 1).TrimEnd();
                _expresionPrevia = $"{sinOperador} {operador} ";
                _operadorPendiente = operador;
                ActualizarPantalla();
                return;
            }

            double valor = Convert.ToDouble(_numeroActual, _cultura);

            if (_operadorPendiente == string.Empty)
            {
                _acumulado = valor;
            }
            else
            {
                _acumulado = Calcular(_acumulado, valor, _operadorPendiente);
                if (!EsValido(_acumulado))
                {
                    MostrarError();
                    return;
                }
                _textoSubtotal = $"Subtotal: {Formatear(_acumulado)}";
            }

            _expresionPrevia += $"{Formatear(valor)} {operador} ";
            _operadorPendiente = operador;
            _numeroActual = string.Empty;
            _sustituirAlEscribir = false;
            ActualizarPantalla();
        }

        private void PulsarIgual()
        {
            if (_hayError || _resultadoMostrado || _operadorPendiente == string.Empty) return;

            double valor = ObtenerValorActual();
            double resultado = Calcular(_acumulado, valor, _operadorPendiente);

            if(!EsValido(resultado))
            {
                MostrarError();
                return;
            }

            _operacionTerminada = _expresionPrevia + Formatear(valor);
            _numeroActual = Formatear(resultado);
            _acumulado = resultado;
            _expresionPrevia = string.Empty;
            _operadorPendiente = string.Empty;
            _textoSubtotal = string.Empty;
            _resultadoMostrado = true;
            ActualizarPantalla();
        }

        private void OnMemoriaSumarClicked(object? sender, EventArgs e)
        {
            if (_hayError) return;
            _memoria = Math.Round(_memoria + ObtenerValorActual(), 10);
            ActualizarMemoria();
            _sustituirAlEscribir = true;
        }

        private void OnMemoriaRestarClicked(object? sender, EventArgs e)
        {
            if (_hayError) return;

            _memoria = Math.Round(_memoria - ObtenerValorActual(), 10);
            ActualizarMemoria();
            _sustituirAlEscribir = true;
        }

        private void OnMemoriaBorrarClicked(object? sender, EventArgs e)
        {
            _memoria = 0;
            ActualizarMemoria();
        }

        private void OnMemoriaRecuperarClicked(object? sender, EventArgs e)
        {
            if (_hayError || _resultadoMostrado) LimpiarOperacion();

            _numeroActual = Formatear(_memoria);
            _sustituirAlEscribir = true;
            ActualizarPantalla();
        }

        private double Calcular(double operandoIzquierdo, double operandoDerecho, string operador)
        {
            double resultado = operador switch
            {
                "+" => operandoIzquierdo + operandoDerecho,
                "-" => operandoIzquierdo - operandoDerecho,
                "×" => operandoIzquierdo * operandoDerecho,
                "÷" => operandoDerecho == 0 ? double.NaN : operandoIzquierdo / operandoDerecho,
                _ => operandoDerecho
            };

            return Math.Round(resultado, 10);
        }

        private bool EsValido(double valor)
        {
            return !double.IsNaN(valor) && !double.IsInfinity(valor);
        }

        private double ObtenerValorActual()
        {
            if (_numeroActual == string.Empty) return _acumulado;
            return Convert.ToDouble(_numeroActual, _cultura);
        }

        private string Formatear(double valor)
        {
            return Convert.ToString(Math.Round(valor, 10), _cultura);
        }

        private void LimpiarOperacion()
        {
            _acumulado = 0;
            _operadorPendiente = string.Empty;
            _expresionPrevia = string.Empty;
            _numeroActual = "0";
            _operacionTerminada = string.Empty;
            _textoSubtotal = string.Empty;
            _resultadoMostrado = false;
            _sustituirAlEscribir = false;
            _hayError = false;
            ActualizarPantalla();
        }

        private void MostrarError()
        {
            LimpiarOperacion();
            _hayError = true;
            ActualizarPantalla();
        }

        private void ActualizarMemoria()
        {
            LblMemoria.Text = $"Mem: {Formatear(_memoria)}";
            LblIndicadorMemoria.IsVisible = _memoria != 0;
        }

        private void ActualizarPantalla()
        {
            string textoSuperior;
            string textoPrincipal;

            if (_hayError)
            {
                textoSuperior = string.Empty;
                textoPrincipal = "Error";
            }
            else if (_resultadoMostrado)
            {
                textoSuperior = $"{_operacionTerminada} =";
                textoPrincipal = _numeroActual;
            }
            else
            {
                textoSuperior = _textoSubtotal;
                textoPrincipal = (_expresionPrevia + _numeroActual).TrimEnd();
            }

            LblOperacion.Text = RecortarPorLaIzquierda(textoSuperior, MaximoCaracteresSuperior);
            LblPantalla.Text = RecortarPorLaIzquierda(textoPrincipal, MaximoCaracteresPrincipal);

            int longitud = LblPantalla.Text.Length;
            LblPantalla.FontSize = longitud <= 10 ? 44
                                 : longitud <= 14 ? 32
                                 : longitud <= 19 ? 24
                                 : 18;
        }

        private string RecortarPorLaIzquierda(string texto, int maximo)
        {
            if (texto.Length <= maximo) return texto;
            return "..." + texto.Substring(texto.Length - (maximo - 1));
        }


#if WINDOWS
        private bool _tecladoConfigurado = false;
        private void ConfigurarTecladoWindows()
        {
            if (_tecladoConfigurado) return;
            if (Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement vista) return;

            for (int i = 0; i <= 9; i++)
            {
                string digito = i.ToString();
                AnadirAtajo(vista, (Windows.System.VirtualKey)((int)Windows.System.VirtualKey.NumberPad0 + i), () => PulsarNumero(digito));
                AnadirAtajo(vista, (Windows.System.VirtualKey)((int)Windows.System.VirtualKey.Number0 + i), () => PulsarNumero(digito));
            }

            AnadirAtajo(vista, Windows.System.VirtualKey.Decimal, () => PulsarComa());
            AnadirAtajo(vista, Windows.System.VirtualKey.Add, () => PulsarOperador("+"));
            AnadirAtajo(vista, Windows.System.VirtualKey.Subtract, () => PulsarOperador("-"));
            AnadirAtajo(vista, Windows.System.VirtualKey.Multiply, () => PulsarOperador("×"));
            AnadirAtajo(vista, Windows.System.VirtualKey.Divide, () => PulsarOperador("÷"));
            AnadirAtajo(vista, Windows.System.VirtualKey.Escape, () => LimpiarOperacion());
            AnadirAtajo(vista, Windows.System.VirtualKey.Delete, () => LimpiarOperacion());

            vista.PreviewKeyDown += (sender, args) => 
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    PulsarIgual();
                    args.Handled = true;
                }

                if (args.Key == Windows.System.VirtualKey.Space)
                {
                    args.Handled = true;
                }
            };

            _tecladoConfigurado = true;
        }

        private void AnadirAtajo(Microsoft.UI.Xaml.UIElement vista, Windows.System.VirtualKey tecla, Action accion)
        {
            var atajo = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = tecla };
            atajo.Invoked += (sender, args) =>
            {
                accion();
                args.Handled = true;
            };
            vista.KeyboardAccelerators.Add(atajo);
        }
#endif
    }
}