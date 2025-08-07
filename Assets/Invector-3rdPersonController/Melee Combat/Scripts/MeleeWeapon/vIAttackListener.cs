namespace Invector.vEventSystems
{
    public interface vIAttackListener
    {
        void OnEnableAttack();

        void OnDisableAttack();

        void ResetAttackTriggers();
    }
}
