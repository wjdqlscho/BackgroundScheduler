from apscheduler.schedulers.background import BackgroundScheduler
import time


def main():
    sched = BackgroundScheduler(timezone='Asia/Seoul')  
    sched.add_job(say_hello, 'interval', seconds=10, id='test')   
    sched.start()  
    print("Scheduler started...")

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("Scheduler stopped...")
        sched.shutdown()

def say_hello():
    print("Hello!")

if __name__ == "__main__":
    main()
