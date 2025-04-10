namespace CineVault.API.Entities;

public sealed class Like
{
    public int Id { get; set; }
    public required int ReviewId { get; set; }
    public required int UserId { get; set; }
    public Review? Review { get; set; }
    public User? User { get; set; }
    // TODO 10 ����������� �������� "�'����� ���������" (Soft Delete) ��� ���������. ������ ���� IsDeleted (property) �� ����� �������
    public bool IsDeleted { get; set; }
}