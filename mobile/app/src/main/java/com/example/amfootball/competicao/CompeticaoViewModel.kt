package com.example.amfootball.competicao

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch

/** Mensagem curta mostrada no ecrã depois de uma ação. */
data class Aviso(val texto: String, val erro: Boolean)

/**
 * Base dos ViewModels de competição: estado de carregamento, erro de carga (ecrã inteiro)
 * e avisos das ações (sem sair do ecrã).
 */
abstract class CompeticaoViewModel : ViewModel() {
    private val _aCarregar = MutableStateFlow(false)
    val aCarregar: StateFlow<Boolean> = _aCarregar.asStateFlow()

    private val _erroCarga = MutableStateFlow<String?>(null)
    val erroCarga: StateFlow<String?> = _erroCarga.asStateFlow()

    private val _aGuardar = MutableStateFlow(false)
    val aGuardar: StateFlow<Boolean> = _aGuardar.asStateFlow()

    private val _aviso = MutableStateFlow<Aviso?>(null)
    val aviso: StateFlow<Aviso?> = _aviso.asStateFlow()

    protected fun carregar(bloco: suspend () -> Unit) {
        viewModelScope.launch {
            _aCarregar.value = true
            _erroCarga.value = null
            try {
                bloco()
            } catch (e: Exception) {
                _erroCarga.value = mensagem(e)
            } finally {
                _aCarregar.value = false
            }
        }
    }

    protected fun acao(sucesso: String?, bloco: suspend () -> Unit) {
        if (_aGuardar.value) return
        viewModelScope.launch {
            _aGuardar.value = true
            _aviso.value = null
            try {
                bloco()
                if (sucesso != null) _aviso.value = Aviso(sucesso, erro = false)
            } catch (e: Exception) {
                _aviso.value = Aviso(mensagem(e), erro = true)
            } finally {
                _aGuardar.value = false
            }
        }
    }

    protected fun avisar(texto: String, erro: Boolean) {
        _aviso.value = Aviso(texto, erro)
    }

    fun limparAviso() {
        _aviso.value = null
    }

    private fun mensagem(e: Exception): String =
        e.message?.takeIf { it.isNotBlank() } ?: "Não foi possível falar com o servidor."
}
