namespace Music.Library.Repository.Model;

public enum OutboxMessageStatus
{
    /// <summary>
    /// Non ancora pubblicato su Kafka, in attesa che il poller lo processi
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Pubblicato con successo su Kafka
    /// </summary>
    Processed = 1,

    /// <summary>
    /// Superato il numero massimo di tentativi: il messaggio non viene più ritentato
    /// automaticamente (funge da dead-letter, resta ispezionabile nella stessa tabella)
    /// </summary>
    Failed = 2
}
