package com.example.amfootball.ui.viewModel.chat

import android.util.Log
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.chat.ChatRoom
import com.example.amfootball.data.dtos.chat.MessageDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Routes
import com.google.firebase.Firebase
import com.google.firebase.auth.auth
import com.google.firebase.firestore.FieldValue
import com.google.firebase.firestore.FirebaseFirestore
import com.google.firebase.firestore.ListenerRegistration
import com.google.firebase.firestore.Query
import com.google.firebase.firestore.firestore
import com.google.firebase.firestore.getField
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do Chat (Listagem de Salas e Conversa Individual).
 *
 * Este componente gere a comunicação em tempo real com o Firebase Firestore para:
 * 1. Obter a lista de salas que o utilizador pertence.
 * 2. Abrir uma "escuta" contínua (`SnapshotListener`) para o histórico de mensagens de uma sala específica.
 * 3. Enviar novas mensagens.
 *
 * @property savedStateHandle Fornecido pelo Hilt, usado para extrair o ID dinâmico da sala (chatRoomId) da rota de navegação.
 */
@HiltViewModel
class ChatViewModel @Inject constructor(
    savedStateHandle: SavedStateHandle,
    sessionManager: SessionManager,
    private val db: FirebaseFirestore
): ViewModel() {
    private val myUser = sessionManager.getUserProfile()
    private val myUserId = myUser?.loginResponseDto?.localId

    private val isAdmin = myUser?.isAdmin


    /**
     * ID da sala de chat atual (extraído dos argumentos de navegação).
     */
    val chatRoomId: String? = savedStateHandle[Routes.chatRoomId]

    /**
     * Estado interno mutável da lista de salas de chat do utilizador.
     */
    private val _rooms = MutableStateFlow<List<ChatRoom>>(emptyList())

    /**
     * Fluxo público de leitura das salas de chat. Observado pelo [ChatListScreen].
     */
    val rooms = _rooms.asStateFlow()

    /**
     * Nome da sala atual (ex: "Equipa A vs Equipa B").
     */
    private val _roomName = MutableStateFlow("")

    val roomName = _roomName.asStateFlow()

    /**
     * Estado interno mutável da lista de mensagens da sala atual.
     */
    private val _messages = MutableStateFlow<List<MessageDto>>(emptyList())

    /**
     * Fluxo público de leitura das mensagens. Observado pelo [ChatScreen] para renderização em tempo real.
     */
    val messages = _messages.asStateFlow()

    /**
     * Registo do listener em tempo real para permitir a remoção ([remove]) no [onCleared] (evitar Memory Leaks).
     */
    private var messagesListener: ListenerRegistration? = null

    init {
        Log.d("ChatViewModel", "fetchMyChatRooms() myUserId=$myUserId")

        if (myUserId != null) {
            fetchMyChatRooms()
        }
    }

    /**
     * Carrega todas as salas de chat em que o utilizador autenticado participa.
     *
     * Utiliza uma query pontual (`.get()`) com `whereArrayContains` para encontrar os documentos
     * da coleção "chatRooms" que contêm o ID do utilizador no campo "members".
     */
    fun fetchMyChatRooms() {
        Log.d("ChatViewModel", "IsAdmin: $isAdmin")
        Log.d("ChatViewModel", "Buscando salas do usuário $myUserId")
        val firebaseAuthUser = com.google.firebase.auth.FirebaseAuth.getInstance().currentUser
        Log.d("ChatDebug", "ID no FirebaseAuth Real: ${firebaseAuthUser?.uid}")
        if (isAdmin == null || myUserId == null) return

        db.collection("chatRooms")
            .whereArrayContains("members", myUserId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                _rooms.value = querySnapshot.toObjects(ChatRoom::class.java)
                Log.d("ChatViewModel", "Salas do usuário $myUserId: ${_rooms.value}")
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }
        Log.d("ChatViewModel", "Salas do usuário $myUserId: ${_rooms.value}")
    }

    /**
     * Abre uma conexão em tempo real (`SnapshotListener`) para ouvir as mensagens da sala atual.
     *
     * **Fluxo:**
     * 1. Remove qualquer listener ativo anteriormente.
     * 2. Faz uma leitura pontual para obter o nome da sala (para a TopBar).
     * 3. Regista o [messagesListener] com ordenação por timestamp (ascendente) e limite de 50 mensagens.
     * 4. Sempre que houver uma nova mensagem no Firestore, o listener atualiza o StateFlow [_messages].
     */
    fun listenForMessages() {
        if (chatRoomId == null) {
            return
        }
        messagesListener?.remove()

        db.collection("chatRooms")
            .document(chatRoomId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                _roomName.value = querySnapshot.getField<String>("name") ?: ""
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }

        val query = db.collection("chatRooms").document(chatRoomId)
            .collection("messages")
            .orderBy("timestamp", Query.Direction.ASCENDING) // ASCENDING = mais antigo primeiro
            .limit(50)

        messagesListener = query.addSnapshotListener { snapshot, error ->
            if (error != null) {
                Log.w("Chat", "Erro ao ouvir mensagens.", error)
                return@addSnapshotListener
            }

            if (snapshot != null) {
                _messages.value = snapshot.toObjects(MessageDto::class.java)
            }
        }
    }

    /**
     * Envia uma nova mensagem para o Firestore.
     *
     * Cria um mapa de dados, anexa o [FieldValue.serverTimestamp()] e adiciona-o à subcoleção "messages".
     *
     * @param messageText O conteúdo da mensagem a ser enviado.
     */
    fun sendMessage(messageText: String) {
        if (myUserId == null) return
        if (isAdmin == null) return
        if (chatRoomId == null) return

        val message = hashMapOf(
            "text" to messageText,
            "senderId" to myUserId,
            "timestamp" to FieldValue.serverTimestamp()
        )

        db.collection("chatRooms").document(chatRoomId)
            .collection("messages")
            .add(message)
            .addOnSuccessListener {
                Log.d("Chat", "Mensagem enviada com sucesso!")
            }
            .addOnFailureListener { e ->
                Log.w("Chat", "Erro ao enviar mensagem", e)
            }
    }

    /**
     * Verifica se uma mensagem específica foi enviada pelo utilizador autenticado.
     *
     * Usado pela UI para determinar o estilo do balão de mensagem (enviada vs recebida).
     *
     * @param message O DTO da mensagem.
     * @return `true` se o [message.senderId] for igual ao ID do utilizador logado.
     */
    fun isSentByMe(message: MessageDto): Boolean {
        return message.senderId == myUserId
    }

    /**
     * Chamado quando o ViewModel é destruído.
     *
     * Essencial para remover o listener de tempo real do Firestore, evitando fugas de memória ([ListenerRegistration.remove]).
     */
    override fun onCleared() {
        super.onCleared()
        Log.d("ChatViewModel", "Limpando listener de mensagens.")
        messagesListener?.remove()
    }
}