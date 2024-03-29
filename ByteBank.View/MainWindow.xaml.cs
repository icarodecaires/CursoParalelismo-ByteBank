﻿using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
          
            var inicio = DateTime.Now;
            var contas = r_Repositorio.GetContaClientes();

            //var resultado = ResolvendoComThreads(contas);

            //Reativação do botão dentro do metodo ResolvendoComTasks
            BtnProcessar.IsEnabled = false;
            var resultado = ResolvendoComTasks(contas);


            //comentado para ser executado dentro no metodo ResolvendoComTasks no WhenAll desta forma não travando a tela principal
            //Descomentar em caso de alteração
            //var fim = DateTime.Now;
            //AtualizarView(resultado, fim - inicio);

           
        }

        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }

        private List<string> ResolvendoComTasksMelhorado(IEnumerable<ContaCliente> contas)
        {
            //TaskScheduler da interface grafica
            var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            var inicio = DateTime.Now;
            var resultado = new List<string>();

            var contasTarefas = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                });
            }).ToArray();


            //Para a tarefa principal para esperar o termino de todas as tasks passadas
            //Task.WaitAll(contasTarefas);

            //Retorna uma task para ser executada e não parar a tarefa principal

            Task.WhenAll(contasTarefas)
                .ContinueWith(task => {
                    var fim = DateTime.Now;
                    AtualizarView(resultado, fim - inicio);
                }, taskSchedulerUI)
                    .ContinueWith(task => {
                        BtnProcessar.IsEnabled = true;
                    }, taskSchedulerUI); //diz para essa tarefa rodar no Scheduler da interface grafica


            return resultado;
        }

        private List<string> ResolvendoComTasks(IEnumerable<ContaCliente> contas)
		{
            //TaskScheduler da interface grafica
           var taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            var inicio = DateTime.Now;
            var resultado = new List<string>();

            var contasTarefas = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                });
            }).ToArray();


            //Para a tarefa principal para esperar o termino de todas as tasks passadas
            //Task.WaitAll(contasTarefas);

            //Retorna uma task para ser executada e não parar a tarefa principal
           
            Task.WhenAll(contasTarefas)
                .ContinueWith(task =>{
                    var fim = DateTime.Now;
                    AtualizarView(resultado, fim - inicio);
                }, taskSchedulerUI)
                    .ContinueWith(task => {
                        BtnProcessar.IsEnabled = true;
                    }, taskSchedulerUI); //diz para essa tarefa rodar no Scheduler da interface grafica


            return resultado;
		}

        private List<string> ResolvendoComThreads(IEnumerable<ContaCliente> contas)
		{
            var contasQuantidadePorThread = contas.Count() / 4;
            var contas_parte1 = contas.Take(contasQuantidadePorThread);
            var contas_parte2 = contas.Skip(contasQuantidadePorThread).Take(contasQuantidadePorThread);
            var contas_parte3 = contas.Skip(contasQuantidadePorThread * 2).Take(contasQuantidadePorThread);
            var contas_parte4 = contas.Skip(contasQuantidadePorThread * 3);

            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            Thread thread_parte1 = new Thread(() =>
            {
                foreach (var conta in contas_parte1)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte2 = new Thread(() =>
            {
                foreach (var conta in contas_parte2)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte3 = new Thread(() =>
            {
                foreach (var conta in contas_parte3)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte4 = new Thread(() =>
            {
                foreach (var conta in contas_parte4)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });

            thread_parte1.Start();
            thread_parte2.Start();
            thread_parte3.Start();
            thread_parte4.Start();

            while (thread_parte1.IsAlive || thread_parte2.IsAlive
                || thread_parte3.IsAlive || thread_parte4.IsAlive)
            {
                Thread.Sleep(250);
                //Não vou fazer nada
            }

            return resultado;
        }
    }
}
