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

@HiltViewModel
class ChatViewModel @Inject constructor(
    savedStateHandle: SavedStateHandle,
    sessionManager: SessionManager,
    private val db: FirebaseFirestore
    ): ViewModel() {
    private val myUser = sessionManager.getUserProfile()
    private val myUserId = myUser?.loginResponseDto?.localId

    private val isAdmin = myUser?.isAdmin


    val chatRoomId: String? = savedStateHandle[Routes.chatRoomId]

    private val _rooms = MutableStateFlow<List<ChatRoom>>(emptyList())
    val rooms = _rooms.asStateFlow() // A UI vai observar isto

    private val _roomName = MutableStateFlow<String>("")

    val roomName = _roomName.asStateFlow()

    private val _messages = MutableStateFlow<List<MessageDto>>(emptyList())
    val messages = _messages.asStateFlow() // A UI vai observar isto

    private var messagesListener: ListenerRegistration? = null

    init {
        if (myUserId != null) {
            fetchMyChatRooms()
        }
    }

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

    fun listenForMessages() {
        if (chatRoomId == null){
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
    fun sendMessage( messageText: String) {
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

    fun isSentByMe(message: MessageDto): Boolean {
        return message.senderId == myUserId
    }


    override fun onCleared() {
        super.onCleared()
        Log.d("ChatViewModel", "Limpando listener de mensagens.")
        messagesListener?.remove()
    }
}