from apscheduler.schedulers.background import BackgroundScheduler
import time


def main():
    sched = BackgroundScheduler(timezone='Asia/Seoul')  # 시간대 설정
    sched.add_job(say_hello, 'interval', seconds=10, id='test')  # 10초 간격으로 say_hello()가 실행됨
    sched.start()  # 스케쥴링 작업 실행
    print("Scheduler started...")

    try:
        # 이 코드는 스케줄러가 실행되는 동안 메인 프로세스가 계속 실행되도록 함
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        # Ctrl+C를 누르면 KeyboardInterrupt가 발생하여 프로그램을 종료합니다.
        print("Scheduler stopped...")
        sched.shutdown()

def say_hello():
    print("Hello!")

if __name__ == "__main__":
    main()